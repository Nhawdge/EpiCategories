﻿using System;
using System.Web.Routing;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Geta.EpiCategories.Routing;

namespace Geta.EpiCategories
{
    [InitializableModule]
    [ModuleDependency(typeof(DataInitialization))]
    public class CategoryInitializationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var contentEvents = context.Locate.ContentEvents();
            contentEvents.CreatingContent += OnCreatingContent;

            RouteTable.Routes.RegisterPartialRouter(new CategoryPartialRouter(context.Locate.ContentLoader(), context.Locate.Advanced.GetInstance<ICategoryContentRepository>()));
        }

        public void Uninitialize(InitializationEngine context)
        {
            ServiceLocator.Current.GetInstance<IContentEvents>().CreatingContent -= OnCreatingContent;
        }

        private void OnCreatingContent(object sender, ContentEventArgs args)
        {
            var categoryData = args.Content as CategoryData;

            if (categoryData != null)
            {
                categoryData.URLSegment = ServiceLocator.Current.GetInstance<IUrlSegmentCreator>().Create(categoryData);
            }
        }
    }
}