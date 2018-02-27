using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Api.DTOs;
using System.Xml;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Media;
using Nop.Plugin.Api.Converters;
using System.Globalization;
using Nop.Core.Data;
using System.Data.Entity;

namespace Nop.Plugin.Api.Services
{
    public class ProductAttributeConverter : IProductAttributeConverter
    {
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;

        public ProductAttributeConverter(IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            IApiTypeConverter apiTypeConverter,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository)
        {
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _downloadService = downloadService;
            _productAttributeRepository = productAttributeRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
        }

        public string ConvertToXml(List<ProductItemAttributeDto> attributeDtos, int productId)
        {
            string attributesXml = "";

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(productId);
            foreach (var attribute in productAttributes)
            {
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            // there should be only one selected value for this attribute
                            var selectedAttribute = attributeDtos.Where(x => x.Id == attribute.Id).FirstOrDefault();
                            if (selectedAttribute != null)
                            {
                                int selectedAttributeValue;
                                bool isInt = int.TryParse(selectedAttribute.Value, out selectedAttributeValue);
                                if (isInt && selectedAttributeValue > 0)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeValue.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            // there could be more than one selected value for this attribute
                            var selectedAttributes = attributeDtos.Where(x => x.Id == attribute.Id);
                            foreach (var selectedAttribute in selectedAttributes)
                            {
                                int selectedAttributeValue;
                                bool isInt = int.TryParse(selectedAttribute.Value, out selectedAttributeValue);
                                if (isInt && selectedAttributeValue > 0)
                                {
                                    // currently there is no support for attribute quantity
                                    var quantity = 1;

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeValue.ToString(), quantity);
                                }

                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only(already server - side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                     attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var selectedAttribute = attributeDtos.Where(x => x.Id == attribute.Id).FirstOrDefault();

                            if (selectedAttribute != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttribute.Value);
                            }

                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var selectedAttribute = attributeDtos.Where(x => x.Id == attribute.Id).FirstOrDefault();

                            if (selectedAttribute != null)
                            {
                                DateTime selectedDate;

                                // Since nopCommerce uses this format to keep the date in the database to keep it consisten we will expect the same format to be passed
                                bool validDate = DateTime.TryParseExact(selectedAttribute.Value, "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate);

                                if (validDate)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedDate.ToString("D"));
                                }
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            var selectedAttribute = attributeDtos.Where(x => x.Id == attribute.Id).FirstOrDefault();

                            if (selectedAttribute != null)
                            {
                                Guid downloadGuid;
                                Guid.TryParse(selectedAttribute.Value, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, download.DownloadGuid.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            // No Gift Card attributes support yet

            return attributesXml;
        }

        public List<ProductItemAttributeDto> Parse(string attributesXml)
        {
            var attributeDtos = new List<ProductItemAttributeDto>();
            if (string.IsNullOrEmpty(attributesXml))
                return attributeDtos;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode attributeNode in xmlDoc.SelectNodes(@"//Attributes/ProductAttribute"))
                {
                    if (attributeNode.Attributes != null && attributeNode.Attributes["ID"] != null)
                    {
                        int attributeId;
                        if (int.TryParse(attributeNode.Attributes["ID"].InnerText.Trim(), out attributeId))
                        {
                            var piaDTO = new ProductItemAttributeDto();
                            foreach (XmlNode attributeValue in attributeNode.SelectNodes("ProductAttributeValue"))
                            {
                                var value = attributeValue.SelectSingleNode("Value").InnerText.Trim();
                                // no support for quantity yet
                                //var quantityNode = attributeValue.SelectSingleNode("Quantity");
                                //attributeDtos.Add(new ProductItemAttributeDto() { Id = attributeId, Value = value });
                                piaDTO.Id = attributeId;
                                piaDTO.Value = value;

                                if (attributeValue.NextSibling == null)
                                {
                                    string attName = "";
                                    var attribute = _productAttributeMappingRepository.Table.Include(x => x.ProductAttribute).FirstOrDefault(x => x.Id == attributeId);
                                    attName = attName + attribute.ProductAttribute.Name;
                                    //ProductAttributeValue attributeValues = new ProductAttributeValue();
                                    if (value != null)
                                    {
                                        int valueId = int.Parse(value);
                                        var attributeValues = _productAttributeValueRepository.Table.FirstOrDefault(x => x.Id == valueId);
                                        attName = attName + ":" + attributeValues.Name;
                                    }
                                    piaDTO.Name = attName;
                                }
                            }

                            foreach (XmlNode attributeValue in attributeNode.SelectNodes("ProductAttributeName"))
                            {
                                var name = attributeValue.SelectSingleNode("Name").InnerText.Trim();
                                //attributeDtos.Add(new ProductItemAttributeDto() { Id = attributeId, Name = name });
                                piaDTO.Name = name;
                            }
                            attributeDtos.Add(piaDTO);
                        }
                    }
                }
            }
            catch { }

            return attributeDtos;
        }
    }
}
