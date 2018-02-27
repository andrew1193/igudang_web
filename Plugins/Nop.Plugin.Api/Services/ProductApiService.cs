using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;

namespace Nop.Plugin.Api.Services
{
    public class ProductApiService : IProductApiService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryMappingRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;

        public ProductApiService(IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryMappingRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<Category> categoryRepository,
            IRepository<ProductReview> productReviewRepository)
        {
            _productRepository = productRepository;
            _productCategoryMappingRepository = productCategoryMappingRepository;
            _vendorRepository = vendorRepository;
            _categoryRepository = categoryRepository;
            _productReviewRepository = productReviewRepository;
        }

        public IList<Product> GetProducts(IList<int> ids = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
           int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId,
           int? categoryId = null, string vendorName = null, string productName = null, string categoryName = null,
           bool? publishedStatus = null, DateTime? eventStartDate = null, DateTime? eventEndDate = null)
        {

            var query = GetProductsQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, vendorName,
                productName, categoryName, publishedStatus, ids, categoryId, eventStartDate, eventEndDate);

            if (sinceId > 0)
            {
                query = query.Where(c => c.Id > sinceId);
            }

            return new ApiList<Product>(query, page - 1, limit);
        }

        public int GetProductsCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, bool? publishedStatus = null, string vendorName = null,
            string productName = null, string categoryName = null, int? categoryId = null, DateTime? eventStartDate = null, DateTime? eventEndDate = null)
        {
            var query = GetProductsQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, vendorName,
                                         productName, categoryName, publishedStatus, categoryId: categoryId, eventStartDate: eventStartDate, eventEndDate: eventEndDate);

            return query.Count();
        }

        public Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

            return _productRepository.Table.FirstOrDefault(product => product.Id == productId && product.Published && !product.Deleted);
        }

        public Product GetProductByIdAndEventDate(int productId, DateTime? start, DateTime? end)
        {
            var product = _productRepository.Table.FirstOrDefault(p => p.Id == productId && p.Published && !p.Deleted && p.AvailableStartDateTimeUtc >= start && p.AvailableEndDateTimeUtc <= end);
            if (product == null)
                return null;

            return product;
        }

        public Product GetProductByIdNoTracking(int productId)
        {
            if (productId == 0)
                return null;

            return _productRepository.TableNoTracking.FirstOrDefault(product => product.Id == productId && !product.Deleted);
        }

        private IQueryable<Product> GetProductsQuery(DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, string vendorName = null, string productName = null,
            string categoryName = null, bool? publishedStatus = null, IList<int> ids = null, int? categoryId = null, DateTime? eventStartDate = null, DateTime? eventEndDate = null)

        {
            var query = _productRepository.TableNoTracking;

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(c => ids.Contains(c.Id));
            }

            if (publishedStatus != null)
            {
                query = query.Where(c => c.Published == publishedStatus.Value);
            }

            // always return products that are not deleted!!!
            query = query.Where(c => !c.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(c => c.CreatedOnUtc > createdAtMin.Value);
            }

            if (createdAtMax != null)
            {
                query = query.Where(c => c.CreatedOnUtc < createdAtMax.Value);
            }

            if (updatedAtMin != null)
            {
                query = query.Where(c => c.UpdatedOnUtc > updatedAtMin.Value);
            }

            if (updatedAtMax != null)
            {
                query = query.Where(c => c.UpdatedOnUtc < updatedAtMax.Value);
            }

            if (!string.IsNullOrEmpty(vendorName))
            {
                query = from vendor in _vendorRepository.TableNoTracking
                        join product in _productRepository.TableNoTracking on vendor.Id equals product.VendorId
                        where vendor.Name == vendorName && !vendor.Deleted && vendor.Active
                        select product;
            }

            if (!string.IsNullOrEmpty(productName))
            {
                query = from p in query
                        where p.Name.Contains(productName) && !p.Deleted && p.Published
                        select p;
            }

            if (!string.IsNullOrEmpty(categoryName))
            {
                var catId = (from c in _categoryRepository.TableNoTracking
                             where c.Name.Contains(categoryName)
                             select c.Id).FirstOrDefault();

                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.TableNoTracking
                                                 where productCategoryMapping.CategoryId == catId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }

            //only distinct products (group by ID)
            query = from p in query
                    group p by p.Id
                        into pGroup
                    orderby pGroup.Key
                    select pGroup.FirstOrDefault();

            if (categoryId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.TableNoTracking
                                                 where productCategoryMapping.CategoryId == categoryId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }

            query = query.Where(x => x.AvailableStartDateTimeUtc != null && x.AvailableEndDateTimeUtc != null);

            if (eventStartDate != null && eventEndDate != null)
            {
                query = query.Where(x => x.AvailableStartDateTimeUtc >= eventStartDate && x.AvailableEndDateTimeUtc <= eventEndDate);
            }

            query = query.OrderBy(product => product.Id);

            return query;
        }

        public IList<ProductReview> GetProductReviews(int productId = 0)
        {
            var query = _productReviewRepository.Table.Where(x => x.ProductId == productId).ToList();

            if (query.Count <= 0)
                return null;

            return query;
        }


    }
}