using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs;
using Nop.Plugin.Api.DTOs.Customers;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.CustomersParameters;
using Nop.Plugin.Api.Serializers;
using Nop.Plugin.Api.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Models.Customer;
using Nop.Core;
using Nop.Services.Orders;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Api.Controllers
{
    //[BearerTokenAuthorize]
    public class CustomersController : BaseApiController
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly ICustomerRolesHelper _customerRolesHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEncryptionService _encryptionService;
        private readonly ICountryService _countryService;
        private readonly IMappingHelper _mappingHelper;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IFactory<Customer> _factory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IAddressService _addressService;

        // We resolve the customer settings this way because of the tests.
        // The auto mocking does not support concreate types as dependencies. It supports only interfaces.
        private CustomerSettings _customerSettings;
        private readonly CustomerSettings _customerSettings2;

        private CustomerSettings CustomerSettings
        {
            get
            {
                if (_customerSettings == null)
                {
                    _customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
                }

                return _customerSettings;
            }
        }

        public CustomersController(
            ICustomerApiService customerApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ICustomerRolesHelper customerRolesHelper,
            IGenericAttributeService genericAttributeService,
            IEncryptionService encryptionService,
            IFactory<Customer> factory,
            ICountryService countryService,
            IMappingHelper mappingHelper,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPictureService pictureService,
            ICustomerRegistrationService customerRegistrationService,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IWorkflowMessageService workflowMessageService,
            IAddressService addressService,
            CustomerSettings customerSettings2) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _customerApiService = customerApiService;
            _factory = factory;
            _countryService = countryService;
            _mappingHelper = mappingHelper;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _encryptionService = encryptionService;
            _genericAttributeService = genericAttributeService;
            _customerRolesHelper = customerRolesHelper;
            _customerRegistrationService = customerRegistrationService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _workflowMessageService = workflowMessageService;
            _customerSettings2 = customerSettings2;
            _addressService = addressService;
        }

        /// <summary>
        /// Retrieve all customers of a shop
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(CustomersRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetCustomers(CustomersParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "Invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid request parameters");
            }

            IList<CustomerDto> allCustomers = _customerApiService.GetCustomersDtos(parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.Limit, parameters.Page, parameters.SinceId);

            var customersRootObject = new CustomersRootObject()
            {
                Customers = allCustomers
            };

            var json = _jsonFieldsSerializer.Serialize(customersRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Retrieve customer by spcified id
        /// </summary>
        /// <param name="id">Id of the customer</param>
        /// <param name="fields">Fields from the customer you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(CustomersRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetCustomerById(int id, string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            CustomerDto customer = _customerApiService.GetCustomerById(id);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            var customersRootObject = new CustomersRootObject();
            customersRootObject.Customers.Add(customer);

            var json = _jsonFieldsSerializer.Serialize(customersRootObject, fields);

            return new RawJsonActionResult(json);
        }


        [HttpGet]
     
        public IHttpActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {

                return ErrorOccured(_localizationService.GetResource("Account.PasswordRecovery.EmailNotFound"));
            }
            else
            {
                var customer = _customerService.GetCustomerByEmail(email);
                if (customer != null)
                {
                    _workContext.CurrentCustomer = customer;

                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.PasswordRecoveryToken,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(customer,
                        SystemCustomerAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                    //send email
                    _workflowMessageService.SendCustomerPasswordRecoveryMessage(customer,
                        _workContext.WorkingLanguage.Id);



                  
                    return Successful("");

                }
                else

                    return ErrorOccured(_localizationService.GetResource("Account.PasswordRecovery.EmailNotFound"));


            }

            
        }



        /// <summary>
        /// Get a count of all customers
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(CustomersCountRootObject))]
        public IHttpActionResult GetCustomersCount()
        {
            var allCustomersCount = _customerApiService.GetCustomersCount();

            var customersCountRootObject = new CustomersCountRootObject()
            {
                Count = allCustomersCount
            };

            return Ok(customersCountRootObject);
        }

        /// <summary>
        /// Search for customers matching supplied query
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        public IHttpActionResult Search(CustomersSearchParametersModel parameters)
        {
            if (parameters.Limit <= Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "Invalid limit parameter");
            }

            if (parameters.Page <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid page parameter");
            }

            //IList<CustomerDto> customersDto = _customerApiService.Search(parameters.Query, parameters.Order, parameters.Page, parameters.Limit);

            var customers = _customerApiService.GetGuestCustomer(parameters.Query);


            var list = new List<CustomerDto>();
            CustomerDto customerDto = new CustomerDto();
            if (customers != null)
            {
                customerDto.Id = customers.Id.ToString();
                customerDto.CustomerGuid = customers.CustomerGuid;
                customerDto.RoleIds.AddRange(customers.CustomerRoles.Select(x => x.Id));
                list.Add(customerDto);
            }
            else
            {
                list = null;
            }


            var customersRootObject = new CustomersRootObject()
            {
                Customers = list
            };

            var json = _jsonFieldsSerializer.Serialize(customersRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [ResponseType(typeof(CustomersRootObject))]
        public IHttpActionResult CreateCustomer([ModelBinder(typeof(JsonModelBinder<CustomerDto>))] Delta<CustomerDto> customerDelta)
        {
            try
            {
                // Here we display the errors if the validation has failed at some point.
                if (!ModelState.IsValid)
                {
                    return Error();
                }

                if (customerDelta.Dto.RoleIds.Contains(3))
                {
                    if (String.IsNullOrEmpty(customerDelta.Dto.Email))
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email is required!");
                    }

                    string reg = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
                    Regex r = new Regex(reg, RegexOptions.IgnoreCase);
                    Match m = r.Match(customerDelta.Dto.Email);
                    if (!m.Success)
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email format invalid!");
                    }

                    if (_customerApiService.CheckIfEmailExist(customerDelta.Dto.Email))
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email already exist!");
                    }

                    if (String.IsNullOrEmpty(customerDelta.Dto.Password))
                    {
                        return Error(HttpStatusCode.BadRequest, "password", "Password is required!");
                    }
                }

                //If the validation has passed the customerDelta object won't be null for sure so we don't need to check for this.

                // Inserting the new customer
                Customer newCustomer = _factory.Initialize();
                customerDelta.Merge(newCustomer);

                foreach (var address in customerDelta.Dto.CustomerAddresses)
                {
                    newCustomer.Addresses.Add(address.ToEntity());
                }

                _customerService.InsertCustomer(newCustomer);

                InsertFirstAndLastNameGenericAttributes(customerDelta.Dto.FirstName, customerDelta.Dto.LastName, newCustomer);

                //password
                if (!string.IsNullOrWhiteSpace(customerDelta.Dto.Password))
                {
                    AddPassword(customerDelta.Dto.Password, newCustomer);
                }

                // We need to insert the entity first so we can have its id in order to map it to anything.
                // TODO: Localization
                // TODO: move this before inserting the customer.
                if (customerDelta.Dto.RoleIds.Count > 0)
                {
                    AddValidRoles(customerDelta, newCustomer);

                    _customerService.UpdateCustomer(newCustomer);
                }

                // Preparing the result dto of the new customer
                // We do not prepare the shopping cart items because we have a separate endpoint for them.
                CustomerDto newCustomerDto = newCustomer.ToDto();

                // This is needed because the entity framework won't populate the navigation properties automatically
                // and the country will be left null. So we do it by hand here.
                PopulateAddressCountryNames(newCustomerDto);

                // Set the fist and last name separately because they are not part of the customer entity, but are saved in the generic attributes.
                newCustomerDto.FirstName = customerDelta.Dto.FirstName;
                newCustomerDto.LastName = customerDelta.Dto.LastName;
                newCustomerDto.CustomerGuid = newCustomer.CustomerGuid;

                newCustomerDto.RoleIds = newCustomer.CustomerRoles.Select(x => x.Id).ToList();

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), newCustomer.Id);

                var customersRootObject = new CustomersRootObject();

                customersRootObject.Customers.Add(newCustomerDto);

                var json = _jsonFieldsSerializer.Serialize(customersRootObject, string.Empty);

                return new RawJsonActionResult(json);
            }
            catch (Exception ex)
            {
                return Error(HttpStatusCode.BadRequest, "error", ex.Message);
            }
        }

        [HttpPut]
        [ResponseType(typeof(CustomersRootObject))]
        public IHttpActionResult UpdateCustomer([ModelBinder(typeof(JsonModelBinder<CustomerDto>))] Delta<CustomerDto> customerDelta)
        {
            try
            {
                // Here we display the errors if the validation has failed at some point.
                if (!ModelState.IsValid)
                {
                    return Error();
                }

                string key = customerDelta.Dto.Id.ToString() + "_" + customerDelta.Dto.FirstName + "_" + customerDelta.Dto.Email + "_U*pDAteCust67505";
                var strMd5 = CommonHelper.CalculateMD5Hash(key);
                if (strMd5 != customerDelta.Dto.ApiKey)
                {
                    return Error(HttpStatusCode.BadRequest, "error", "key not match");
                }

                //If the validation has passed the customerDelta object won't be null for sure so we don't need to check for this.

                // Updateting the customer
                Customer currentCustomer = _customerApiService.GetCustomerEntityById(int.Parse(customerDelta.Dto.Id));

                if (currentCustomer == null)
                {
                    return Error(HttpStatusCode.NotFound, "customer", "not found");
                }

                bool WelcomeMessageEnable = false;
                if (currentCustomer.CustomerRoles.Any(x => x.Id == 3))
                {
                    if (String.IsNullOrEmpty(customerDelta.Dto.Email))
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email is required!");
                    }

                    string reg = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
                    Regex r = new Regex(reg, RegexOptions.IgnoreCase);
                    Match m = r.Match(customerDelta.Dto.Email);
                    if (!m.Success)
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email format invalid!");
                    }

                    if (customerDelta.Dto.Email != currentCustomer.Email)
                    {
                        if (_customerApiService.CheckIfEmailExistForUpdate(customerDelta.Dto.Email, currentCustomer.Id))
                        {
                            return Error(HttpStatusCode.BadRequest, "email", "Email already exist!");
                        }
                    }
                }
                else if (currentCustomer.CustomerRoles.Any(x => x.Id == 4))
                {
                    if (String.IsNullOrEmpty(customerDelta.Dto.Email))
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email is required!");
                    }

                    string reg = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
                    Regex r = new Regex(reg, RegexOptions.IgnoreCase);
                    Match m = r.Match(customerDelta.Dto.Email);
                    if (!m.Success)
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email format invalid!");
                    }

                    if (_customerApiService.CheckIfEmailExistForUpdate(customerDelta.Dto.Email, currentCustomer.Id))
                    {
                        return Error(HttpStatusCode.BadRequest, "email", "Email already exist!");
                    }

                    WelcomeMessageEnable = true;
                }

                //customerDelta.Merge(currentCustomer);
                _genericAttributeService.SaveAttribute(currentCustomer, SystemCustomerAttributeNames.FirstName, customerDelta.Dto.FirstName);
                _genericAttributeService.SaveAttribute(currentCustomer, SystemCustomerAttributeNames.LastName, customerDelta.Dto.LastName);
                currentCustomer.Email = customerDelta.Dto.Email;

                if (customerDelta.Dto.RoleIds.Count > 0)
                {
                    // Remove all roles
                    while (currentCustomer.CustomerRoles.Count > 0)
                    {
                        currentCustomer.CustomerRoles.Remove(currentCustomer.CustomerRoles.First());
                    }

                    AddValidRoles(customerDelta, currentCustomer);
                }

                if (customerDelta.Dto.CustomerAddresses.Count > 0)
                {
                    var currentCustomerAddresses = currentCustomer.Addresses.ToDictionary(address => address.Id, address => address);

                    foreach (var passedAddress in customerDelta.Dto.CustomerAddresses)
                    {
                        int passedAddressId = int.Parse(passedAddress.Id);
                        Address addressEntity = passedAddress.ToEntity();

                        if (currentCustomerAddresses.ContainsKey(passedAddressId))
                        {
                            _mappingHelper.Merge(passedAddress, currentCustomerAddresses[passedAddressId]);
                        }
                        else
                        {
                            currentCustomer.Addresses.Add(addressEntity);
                        }
                    }
                }

                if (customerDelta.Dto.BillingAddress != null)
                {
                    if (currentCustomer.BillingAddress != null) /*edit billing address*/
                    {
                        var oldBillAddress = currentCustomer.BillingAddress;
                        if (oldBillAddress != null)
                        {
                            oldBillAddress.FirstName = customerDelta.Dto.BillingAddress.FirstName;
                            oldBillAddress.LastName = customerDelta.Dto.BillingAddress.LastName;
                            oldBillAddress.City = customerDelta.Dto.BillingAddress.City;
                            oldBillAddress.ZipPostalCode = customerDelta.Dto.BillingAddress.ZipPostalCode;
                            oldBillAddress.Address1 = customerDelta.Dto.BillingAddress.Address1;
                            oldBillAddress.Address2 = customerDelta.Dto.BillingAddress.Address2;
                            oldBillAddress.Email = customerDelta.Dto.BillingAddress.Email;
                            oldBillAddress.CountryId = customerDelta.Dto.BillingAddress.CountryId;
                            oldBillAddress.StateProvinceId = customerDelta.Dto.BillingAddress.StateProvinceId;
                            oldBillAddress.PhoneNumber = customerDelta.Dto.BillingAddress.PhoneNumber;
                            _addressService.UpdateAddress(oldBillAddress);
                        }
                    }
                    else/*create billing address*/
                    {
                        var newBillAddress = customerDelta.Dto.BillingAddress.ToEntity();
                        _addressService.InsertAddress(newBillAddress);
                        currentCustomer.BillingAddress = newBillAddress;
                    }
                }

                if (customerDelta.Dto.ShippingAddress != null)
                {
                    if (currentCustomer.ShippingAddress != null)/*edit shipping address*/
                    {
                        var oldShipAddress = currentCustomer.ShippingAddress;
                        if (oldShipAddress != null)
                        {
                            oldShipAddress.FirstName = customerDelta.Dto.ShippingAddress.FirstName;
                            oldShipAddress.LastName = customerDelta.Dto.ShippingAddress.LastName;
                            oldShipAddress.City = customerDelta.Dto.ShippingAddress.City;
                            oldShipAddress.ZipPostalCode = customerDelta.Dto.ShippingAddress.ZipPostalCode;
                            oldShipAddress.Address1 = customerDelta.Dto.ShippingAddress.Address1;
                            oldShipAddress.Address2 = customerDelta.Dto.ShippingAddress.Address2;
                            oldShipAddress.Email = customerDelta.Dto.ShippingAddress.Email;
                            oldShipAddress.CountryId = customerDelta.Dto.ShippingAddress.CountryId;
                            oldShipAddress.StateProvinceId = customerDelta.Dto.ShippingAddress.StateProvinceId;
                            oldShipAddress.PhoneNumber = customerDelta.Dto.ShippingAddress.PhoneNumber;
                            _addressService.UpdateAddress(oldShipAddress);
                        }
                    }
                    else/*create shipping address*/
                    {
                        var newShipAddress = customerDelta.Dto.ShippingAddress.ToEntity();
                        _addressService.InsertAddress(newShipAddress);
                        currentCustomer.ShippingAddress = newShipAddress;
                    }
                }

                _customerService.UpdateCustomer(currentCustomer);

                InsertFirstAndLastNameGenericAttributes(customerDelta.Dto.FirstName, customerDelta.Dto.LastName, currentCustomer);

                //password
                if (!string.IsNullOrWhiteSpace(customerDelta.Dto.Password))
                {
                    AddPassword(customerDelta.Dto.Password, currentCustomer);
                }

                // TODO: Localization

                // Preparing the result dto of the new customer
                // We do not prepare the shopping cart items because we have a separate endpoint for them.
                CustomerDto updatedCustomer = currentCustomer.ToDto();

                // This is needed because the entity framework won't populate the navigation properties automatically
                // and the country name will be left empty because the mapping depends on the navigation property
                // so we do it by hand here.
                PopulateAddressCountryNames(updatedCustomer);

                // Set the fist and last name separately because they are not part of the customer entity, but are saved in the generic attributes.
                var firstNameGenericAttribute = _genericAttributeService.GetAttributesForEntity(currentCustomer.Id, typeof(Customer).Name)
                    .FirstOrDefault(x => x.Key == "FirstName");

                if (firstNameGenericAttribute != null)
                {
                    updatedCustomer.FirstName = firstNameGenericAttribute.Value;
                }

                var lastNameGenericAttribute = _genericAttributeService.GetAttributesForEntity(currentCustomer.Id, typeof(Customer).Name)
                    .FirstOrDefault(x => x.Key == "LastName");

                if (lastNameGenericAttribute != null)
                {
                    updatedCustomer.LastName = lastNameGenericAttribute.Value;
                }

                updatedCustomer.RoleIds = currentCustomer.CustomerRoles.Select(x => x.Id).ToList();

                //activity log
                _customerActivityService.InsertActivity("UpdateCustomer", _localizationService.GetResource("ActivityLog.UpdateCustomer"), currentCustomer.Id);

                //Send message to new user
                if (WelcomeMessageEnable)
                {
                    _workflowMessageService.SendCustomerWelcomeMessage(currentCustomer, _workContext.WorkingLanguage.Id);
                }

                var customersRootObject = new CustomersRootObject();

                customersRootObject.Customers.Add(updatedCustomer);

                var json = _jsonFieldsSerializer.Serialize(customersRootObject, string.Empty);

                return new RawJsonActionResult(json);
            }
            catch (Exception ex)
            {
                return Error(HttpStatusCode.BadRequest, "error", ex.Message);
            }
        }

        [HttpDelete]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult DeleteCustomer(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            Customer customer = _customerApiService.GetCustomerEntityById(id);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            _customerService.DeleteCustomer(customer);

            //remove newsletter subscription (if exists)
            foreach (var store in _storeService.GetAllStores())
            {
                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, store.Id);
                if (subscription != null)
                    _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
            }

            //activity log
            _customerActivityService.InsertActivity("DeleteCustomer", _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);

            return new RawJsonActionResult("{}");
        }

        private void InsertFirstAndLastNameGenericAttributes(string firstName, string lastName, Customer newCustomer)
        {
            // we assume that if the first name is not sent then it will be null and in this case we don't want to update it
            if (firstName != null)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.FirstName, firstName);
            }

            if (lastName != null)
            {
                _genericAttributeService.SaveAttribute(newCustomer, SystemCustomerAttributeNames.LastName, lastName);
            }
        }

        private void AddValidRoles(Delta<CustomerDto> customerDelta, Customer currentCustomer)
        {
            IList<CustomerRole> validCustomerRoles =
                _customerRolesHelper.GetValidCustomerRoles(customerDelta.Dto.RoleIds).ToList();

            // Add all newly passed roles
            foreach (var role in validCustomerRoles)
            {
                currentCustomer.CustomerRoles.Add(role);
            }
        }

        private void PopulateAddressCountryNames(CustomerDto newCustomerDto)
        {
            foreach (var address in newCustomerDto.CustomerAddresses)
            {
                SetCountryName(address);
            }

            if (newCustomerDto.BillingAddress != null)
            {
                SetCountryName(newCustomerDto.BillingAddress);
            }

            if (newCustomerDto.ShippingAddress != null)
            {
                SetCountryName(newCustomerDto.ShippingAddress);
            }
        }

        private void SetCountryName(AddressDto address)
        {
            if (string.IsNullOrEmpty(address.CountryName) && address.CountryId.HasValue)
            {
                Country country = _countryService.GetCountryById(address.CountryId.Value);
                address.CountryName = country.Name;
            }
        }

        private void AddPassword(string newPassword, Customer customer)
        {
            // TODO: call this method before inserting the customer.
            var customerPassword = new CustomerPassword
            {
                Customer = customer,
                PasswordFormat = CustomerSettings.DefaultPasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };

            switch (CustomerSettings.DefaultPasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        customerPassword.Password = newPassword;
                    }
                    break;
                case PasswordFormat.Encrypted:
                    {
                        customerPassword.Password = _encryptionService.EncryptText(newPassword);
                    }
                    break;
                case PasswordFormat.Hashed:
                    {
                        string saltKey = _encryptionService.CreateSaltKey(5);
                        customerPassword.PasswordSalt = saltKey;
                        customerPassword.Password = _encryptionService.CreatePasswordHash(newPassword, saltKey, CustomerSettings.HashedPasswordFormat);
                    }
                    break;
            }

            _customerService.InsertCustomerPassword(customerPassword);

            // TODO: remove this.
            _customerService.UpdateCustomer(customer);
        }


        [HttpPost]
        public IHttpActionResult GetCustomerLogin(RegisterModel model)
        {

            try
            {

                var customer = _customerSettings2.UsernamesEnabled ? _customerService.GetCustomerByUsername(model.Username) : _customerService.GetCustomerByEmail(model.Email);

                var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings2.UsernamesEnabled ? model.Username : model.Email, model.Password);
                if (loginResult == CustomerLoginResults.Successful)
                {


                    //var current = _customerService.GetCustomerByUsername(model.Username);
                    //if (current.MLMrequirePass == null || current.MLMrequirePass == "")
                    //{
                    //    string updateemptyMLMpass = _encryptservice.EncryptText(model.Password);
                    //    current.MLMrequirePass = updateemptyMLMpass;

                    //    _customerService.UpdateCustomer(current);
                    //}

                    //migrate shopping cart
                    if (model.LastIpAddress != null)
                    {
                        var guestCustomer = _customerApiService.GetGuestCustomer(model.LastIpAddress);
                        if (guestCustomer != null)
                        {
                            _shoppingCartService.MigrateShoppingCart(guestCustomer, customer, true);
                        }

                    }


                    //sign in new customer
                    //_authenticationService.SignIn(customer, model.RememberMe);

                    //activity log
                    _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);


                    var phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);

                    //sync profile
                    var currentLogin = _customerService.GetCustomerByUsername(model.Username);
                    //if (currentLogin.MLMrequirePass != "")
                    //{
                    //    var descryptPassword = _encryptservice.DecryptText(currentLogin.MLMrequirePass);

                    //    _eBossMLM.GetMemberProfile(model.Username, descryptPassword);
                    //}

                    return Successful(GetCustomerJson(customer));


                }
                //else if (loginResult == CustomerLoginResults.NotRegistered || loginResult == CustomerLoginResults.CustomerNotExist)
                //{
                //    //check  this user is exist in ebosMLM
                //    var success = _eBossMLM.MemberLogin(model.Username, model.Password);
                //    if (success == CustomerLoginResults.Successful)
                //    {

                //        //step1= if exist in mlm system, sync to our  system
                //        var profile = _eBossMLM.GetMemberProfileObj(model.Username, model.Password);

                //        if (profile != null)
                //        {
                //            var registerModel = new RegisterModel();
                //            registerModel.Username = profile.display_name;
                //            registerModel.FirstName = profile.display_name;
                //            registerModel.Email = profile.EMail;
                //            registerModel.Password = model.Password;
                //            registerModel.SponsorName = profile.Sponsor_username;
                //            registerModel.MainWallet = profile.Balance;
                //            registerModel.BonusWallet = profile.BonusBalance;
                //            registerModel.Phone = profile.MobileNo;

                //            return RegisterMemberWithoutSync(registerModel);




                //        }

                //        return ErrorOccured(_localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));




                //    }
                //    else
                //    {
                //        return ErrorOccured(_localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                //    }




                //}
                else if (loginResult == CustomerLoginResults.Deleted)
                {
                    return ErrorOccured(
                     _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));

                }
                else if (loginResult == CustomerLoginResults.Deleted)
                {
                    return ErrorOccured(
                      _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));


                }
                else if (loginResult == CustomerLoginResults.NotActive)
                {
                    return ErrorOccured(
                        _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));

                }

                else if (loginResult == CustomerLoginResults.NotRegistered)
                {
                    return ErrorOccured(
                        _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));


                }

                else
                {
                    return ErrorOccured(
                        _localizationService.GetResource("Account.Login.WrongCredentials"));

                }


            }
            catch (Exception ex)
            {
                return Error(HttpStatusCode.BadRequest, "login", ex.ToString());
            }


        }

        private object GetCustomerJson(Customer customer)
        {
            // TODO: refactor into own method for reuse
            var customerRoles = customer.CustomerRoles
                .Select(c =>
                    new
                    {
                        Id = c.Id,
                        Name = c.Name,
                        SystemName = c.SystemName
                    });

            // TODO: refactor into own method for reuse

            var externalAuthenticationRecords = customer.ExternalAuthenticationRecords
                .Select(e =>
                    new
                    {
                        Id = e.Id,
                        CustomerId = e.CustomerId,
                        Email = e.Email,
                        ExternalIdentifier = e.ExternalIdentifier,
                        ExternalDisplayIdentifier = e.ExternalDisplayIdentifier,
                        OAuthToken = e.OAuthToken,
                        OAuthAccessToken = e.OAuthAccessToken,
                        ProviderSystemName = e.ProviderSystemName
                    });

            // TODO: refactor into own method for reuse
            var shoppingCartItem = customer.ShoppingCartItems
                .Select(c =>
                    new
                    {
                        Id = c.Id,
                        StoreId = c.StoreId,
                        ShoppingCartTypeId = c.ShoppingCartTypeId,
                        CustomerId = c.CustomerId,
                        ProductId = c.ProductId,
                        AttributesXml = c.AttributesXml,
                        CustomerEnteredPrice = c.CustomerEnteredPrice,
                        Quantity = c.Quantity,
                        CreatedOnUtc = c.CreatedOnUtc,
                        UpdatedOnUtc = c.UpdatedOnUtc,
                        IsFreeShipping = c.IsFreeShipping,
                        IsShipEnabled = c.IsShipEnabled,
                        AdditionalShippingCharge = c.AdditionalShippingCharge,
                        IsTaxExempt = c.IsTaxExempt
                    });

            var dob = "";
            var dobFormat = "";
            if (!string.IsNullOrEmpty(customer.GetAttribute<string>(SystemCustomerAttributeNames.DateOfBirth)))
            {
                dob = customer.GetAttribute<string>(SystemCustomerAttributeNames.DateOfBirth);
                dobFormat = Convert.ToDateTime(customer.GetAttribute<string>(SystemCustomerAttributeNames.DateOfBirth)).ToString("dd/MM/yyyy");
            }



            var customerJson = new
            {
                Id = customer.Id,
                CustomerGuid = customer.CustomerGuid,
                UserName = customer.Username,
                Email = customer.Email,
                FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender),
                DateOfBirth = dob,
                DateOfBirthFormat = dobFormat,
                Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),


                FullName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName) + " " + customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName)


            };

            return customerJson;
        }
    }
}