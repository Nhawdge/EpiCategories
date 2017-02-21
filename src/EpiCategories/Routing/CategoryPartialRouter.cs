﻿using System;
using System.Globalization;
using System.Web.Routing;
using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;

namespace Geta.EpiCategories.Routing
{
    public class CategoryPartialRouter : IPartialRouter<ICategoryRoutableContent, PageData>
    {
        protected readonly IContentLoader ContentLoader;
        protected readonly ICategoryContentRepository CategoryRepository;

        public CategoryPartialRouter(IContentLoader contentLoader, ICategoryContentRepository categoryRepository)
        {
            ContentLoader = contentLoader;
            CategoryRepository = categoryRepository;
        }

        public object RoutePartial(ICategoryRoutableContent content, SegmentContext segmentContext)
        {
            var thisSegment = segmentContext.RemainingPath;
            var nextSegment = segmentContext.GetNextValue(segmentContext.RemainingPath);

            while (string.IsNullOrEmpty(nextSegment.Remaining) == false)
            {
                nextSegment = segmentContext.GetNextValue(nextSegment.Remaining);
            }

            if (string.IsNullOrWhiteSpace(nextSegment.Next) == false)
            {
                var localizableContent = content as ILocale;
                CultureInfo preferredCulture = localizableContent?.Language ?? ContentLanguage.PreferredCulture;
                var category = CategoryRepository.GetFirstBySegment<CategoryData>(nextSegment.Next, preferredCulture);

                if (category == null)
                    return null;

                segmentContext.RemainingPath = thisSegment.Substring(0, thisSegment.LastIndexOf(nextSegment.Next, StringComparison.InvariantCultureIgnoreCase));
                segmentContext.RouteData.Values.Add(CategoryRoutingConstants.CurrentCategory, category);
                return content;
            }

            return null;
        }

        public PartialRouteData GetPartialVirtualPath(PageData content, string language, RouteValueDictionary routeValues, RequestContext requestContext)
        {
            object currentCategory;

            if (content is ICategoryRoutableContent == false || routeValues.TryGetValue(CategoryRoutingConstants.CurrentCategory, out currentCategory) == false)
                return null;

            ContentReference categoryLink = (ContentReference) currentCategory;
            CategoryData category;

            if (CategoryRepository.TryGet(categoryLink, out category) == false)
                return null;

            routeValues.Remove(CategoryRoutingConstants.CurrentCategory);

            return new PartialRouteData
            {
                BasePathRoot = content.ContentLink,
                PartialVirtualPath = $"{category.URLSegment}/"
            };
        }
    }
}