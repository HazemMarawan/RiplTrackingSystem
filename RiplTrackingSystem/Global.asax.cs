using AutoMapper;
using RiplTrackingSystem.Models;
using RiplTrackingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RiplTrackingSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure();

            Mapper.CreateMap<PermissionViewModel, Permission>();
            Mapper.CreateMap<RoleViewModel, Role>();
            Mapper.CreateMap<PermissionGroupViewModel, PermissionGroup>();
            Mapper.CreateMap<UserViewModel, User>();
            Mapper.CreateMap<locationViewModel, Location>();
            //Mapper.CreateMap<User, UserViewModel>();
            Mapper.CreateMap<AssetViewModel, Asset>();
            Mapper.CreateMap<RentOrderViewModel, RentOrder>();
            Mapper.CreateMap<RequestViewModel, Request>();
            Mapper.CreateMap<CompanyAssetRent, CompanyAssetRentHistory>();
            Mapper.CreateMap<EmailViewModel, Email>();
            Mapper.CreateMap<NoteViewModel, Note>();
            Mapper.CreateMap<TaskViewModel, Task>();
            Mapper.CreateMap<LocationAttachmentViewModel, LocationAttachment>();

        }
    }
}
