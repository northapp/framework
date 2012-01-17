﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using Signum.Utilities;
    using Signum.Entities;
    using Signum.Web;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Caching;
    using System.Web.DynamicData;
    using System.Web.SessionState;
    using System.Web.Profile;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.UI.HtmlControls;
    using System.Xml.Linq;
    using Signum.Web.Properties;
    using Signum.Entities.DynamicQuery;
    using Signum.Engine.DynamicQuery;
    using Signum.Entities.Reflection;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MvcRazorClassGenerator", "1.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Signum/Views/SearchControl.cshtml")]
    public class _Page_Signum_Views_SearchControl_cshtml : System.Web.Mvc.WebViewPage<Context>
    {


        public _Page_Signum_Views_SearchControl_cshtml()
        {
        }
        protected System.Web.HttpApplication ApplicationInstance
        {
            get
            {
                return ((System.Web.HttpApplication)(Context.ApplicationInstance));
            }
        }
        public override void Execute()
        {








   
    Model.ReadOnly = false; /*SearchControls Context should never inherit Readonly property of parent context */
    FindOptions findOptions = (FindOptions)ViewData[ViewDataKeys.FindOptions];
    QueryDescription queryDescription = (QueryDescription)ViewData[ViewDataKeys.QueryDescription];
    var entityColumn = queryDescription.Columns.SingleEx(a => a.IsEntity);
    Type entitiesType = Reflector.ExtractLite(entityColumn.Type);
    Implementations implementations = entityColumn.Implementations.Value;
    bool viewable = findOptions.View && (implementations.IsByAll ? true : implementations.Types.Any(t => Navigator.IsViewable(t, EntitySettingsContext.Admin)));
    bool? allowMultiple = findOptions.AllowMultiple;
    if (allowMultiple == null && findOptions.FilterMode != FilterMode.OnlyResults)
    {
        allowMultiple = Navigator.QuerySettings(findOptions.QueryName).AllowMultiple;
    }


WriteLiteral("<div id=\"");


    Write(Model.Compose("divSearchControl"));

WriteLiteral("\" \r\n     class=\"sf-search-control\" \r\n     data-prefix=\"");


             Write(Model.ControlID);

WriteLiteral("\" \r\n     data-quickfilter-url=\"");


                      Write(Url.SignumAction("QuickFilter"));

WriteLiteral("\"\r\n     data-search-url=\"");


                 Write(Url.SignumAction("Search"));

WriteLiteral("\"\r\n     data-popup-save-url=\"");


                     Write(Url.SignumAction("TrySavePartial"));

WriteLiteral("\"\r\n     data-add-filter-url=\"");


                     Write(Url.Action("AddFilter", "Signum"));

WriteLiteral("\"\r\n     ");


 Write(findOptions.EntityContextMenu ? " data-entity-ctx-menu-url=" + Url.SignumAction("GetContextualPanel") : "");

WriteLiteral("\r\n     >\r\n\r\n    ");


Write(Html.Hidden(Model.Compose("sfWebQueryName"), Navigator.ResolveWebQueryName(findOptions.QueryName), new { disabled = "disabled" }));

WriteLiteral("\r\n    ");


Write(Html.Hidden(Model.Compose("sfAllowMultiple"), allowMultiple.ToString(), new { disabled = "disabled" }));

WriteLiteral("\r\n    ");


Write(Html.Hidden(Model.Compose("sfView"), viewable, new { disabled = "disabled" }));

WriteLiteral("\r\n    ");


Write(Html.Hidden(Model.Compose("sfFilterMode"), findOptions.FilterMode.ToString(), new { disabled = "disabled" }));

WriteLiteral("\r\n    ");


Write(Html.Hidden(Model.Compose("sfOrders"), findOptions.OrderOptions.IsEmpty() ? "" :
        (findOptions.OrderOptions.ToString(oo => (oo.OrderType == OrderType.Ascending ? "" : "-") + oo.Token.FullKey(), ";") + ";")));

WriteLiteral("\r\n    ");


Write(Html.Hidden(Model.Compose("sfEntityTypeNames"), 
                                implementations.IsByAll ? "[All]" :
                                implementations.Types.ToString(t => Navigator.ResolveWebTypeName(t), ","), 
        new { disabled = "disabled" }));

WriteLiteral("\r\n    \r\n");


     if(findOptions.SearchOnLoad)
    {

WriteLiteral("        <script type=\"text/javascript\">\r\n            $(document).ready(function (" +
") { { SF.FindNavigator.searchOnLoad(\'");


                                                                        Write(Model.ControlID);

WriteLiteral("\'); } });\r\n            </script>    \r\n");


    }

WriteLiteral("    \r\n");


      
        bool filtersAlwaysHidden = findOptions.FilterMode == FilterMode.AlwaysHidden || findOptions.FilterMode == FilterMode.OnlyResults;
        bool filtersVisible = findOptions.FilterMode == FilterMode.Visible;
     

WriteLiteral("    \r\n    <div style=\"display:");


                    Write(filtersAlwaysHidden ? "none" : "block");

WriteLiteral("\">\r\n        <div class=\"sf-fields-list\">\r\n            <div class=\"ui-widget sf-fi" +
"lters\" ");


                                          Write(filtersVisible ? "" : "style=display:none");

WriteLiteral(">\r\n                <div class=\"ui-widget-header ui-corner-top sf-filters-body\">\r\n" +
"                    ");


               Write(Html.TokenOptionsCombo(queryDescription.QueryName, SearchControlHelper.RootTokensCombo(queryDescription, null), Model, 0, false));

WriteLiteral("\r\n                \r\n                    ");


               Write(Html.Href(
                            Model.Compose("btnAddFilter"), 
                            Resources.FilterBuilder_AddFilter, 
                            "",
                            Resources.Signum_selectToken,
                            "sf-query-button sf-add-filter sf-disabled", 
                            new Dictionary<string, object> 
                            { 
                                { "data-icon", "ui-icon-arrowthick-1-s" },
                                { "data-url", Url.SignumAction("AddFilter") }
                            }));

WriteLiteral("\r\n\r\n");


                     if (findOptions.AllowUserColumns.HasValue ? findOptions.AllowUserColumns.Value : Navigator.Manager.AllowUserColumns(Model.ControlID))
                    {
                        
                   Write(Html.Href(
                            Model.Compose("btnAddColumn"), 
                            Resources.FilterBuilder_AddColumn, 
                            "",
                            Resources.Signum_selectToken,
                            "sf-query-button sf-add-column sf-disabled", 
                            new Dictionary<string, object> 
                            { 
                                { "data-icon", "ui-icon-arrowthick-1-e" },
                                { "data-url", Url.SignumAction("GetColumnName") }
                            }));

                              
                    }

WriteLiteral("                </div>\r\n");


                   
                    ViewData[ViewDataKeys.FilterOptions] = findOptions.FilterOptions;
                    Html.RenderPartial(Navigator.Manager.FilterBuilderView); 
                

WriteLiteral("            </div>\r\n        </div>\r\n    </div>\r\n    \r\n");


     if (!filtersAlwaysHidden)
    {
        
   Write(Html.Href("",
                (filtersVisible ? Resources.Signum_hideFilters : Resources.Signum_showFilters),
                "",
                (filtersVisible ? Resources.Signum_hideFilters : Resources.Signum_showFilters),
                "sf-query-button" + " sf-filters-header" + (filtersVisible ? "" : " close"),
                new Dictionary<string, object> 
                { 
                    { "onclick", "new SF.FindNavigator({{prefix: '{0}'}}).toggleFilters(this)".Formato(Model.ControlID) },
                    { "data-icon", filtersVisible ? "ui-icon-triangle-1-n" : "ui-icon-triangle-1-e" }
                }));

                  
    }

WriteLiteral("    \r\n    <div class=\"sf-query-button-bar\" style=\"display:");


                                                Write((findOptions.FilterMode != FilterMode.OnlyResults) ? "block" : "none");

WriteLiteral("\">\r\n        <button type=\"submit\" class=\"sf-query-button sf-search\" data-icon=\"ui" +
"-icon-search\" id=\"");


                                                                                          Write(Model.Compose("qbSearch"));

WriteLiteral("\" onclick=\"");


                                                                                                                                Write("new SF.FindNavigator({{prefix:'{0}',searchControllerUrl:'{1}'}}).search();return false;".Formato(Model.ControlID, Url.SignumAction("Search")));

WriteLiteral("\">");


                                                                                                                                                                                                                                                                                  Write(Resources.Search);

WriteLiteral("</button>\r\n");


         if (findOptions.Create && !implementations.IsByAll && implementations.Types.Any(t=> Navigator.IsCreable(t, EntitySettingsContext.Admin)) && viewable)
        {
            bool hasManyImplementations = implementations.Types.Length > 1;
            string creating = findOptions.Creating.HasText() ? findOptions.Creating :
                "SF.FindNavigator.create({{prefix:'{0}',controllerUrl:'{1}'}},'{2}');return false;".Formato(
                    Model.ControlID, 
                    Url.SignumAction(string.IsNullOrEmpty(Model.ControlID) ? "Create" : "PopupCreate"),
                    hasManyImplementations ? RouteHelper.New().SignumAction("GetTypeChooser") : "");

WriteLiteral("            <a class=\"sf-query-button\" data-icon=\"ui-icon-plusthick\" data-text=\"f" +
"alse\" id=\"");


                                                                                      Write(Model.Compose("qbSearchCreate"));

WriteLiteral("\" onclick=\"");


                                                                                                                                 Write(creating);

WriteLiteral("\">");


                                                                                                                                            Write(Resources.Search_Create);

WriteLiteral("</a>\r\n");


        }

WriteLiteral("        ");


   Write(ButtonBarQueryHelper.GetButtonBarElementsForQuery(this.ViewContext, findOptions.QueryName, entitiesType, Model.ControlID).ToString(Html));

WriteLiteral("\r\n    </div>\r\n");


     if (findOptions.FilterMode != FilterMode.OnlyResults)
    {

WriteLiteral("        <div class=\"clearall\">\r\n        </div>\r\n");


    }

WriteLiteral("    <div id=\"");


        Write(Model.Compose("divResults"));

WriteLiteral("\" class=\"ui-widget ui-corner-all sf-search-results-container\">\r\n        <table id" +
"=\"");


              Write(Model.Compose("tblResults"));

WriteLiteral("\" class=\"sf-search-results\">\r\n            <thead class=\"ui-widget-header ui-corne" +
"r-top\">\r\n                <tr>\r\n");


                     if (allowMultiple.HasValue)
                    {

WriteLiteral("                        <th class=\"ui-state-default th-col-selection\">\r\n");


                             if (allowMultiple.Value)
                            {
                                
                           Write(Html.CheckBox(Model.Compose("cbSelectAll"), false, new { onclick = "javascript:new SF.FindNavigator({{prefix:'{0}'}}).toggleSelectAll();".Formato(Model.ControlID) }));

                                                                                                                                                                                                      
                            }

WriteLiteral("                        </th>\r\n");


                    }


                     if (viewable)
                    {

WriteLiteral("                        <th class=\"ui-state-default th-col-entity\">\r\n            " +
"            </th>\r\n");


                    }


                      List<Column> columns = findOptions.MergeColumns(); 


                     foreach (var col in columns)
                    {
                        var order = findOptions.OrderOptions.FirstOrDefault(oo => oo.Token.FullKey() == col.Name);
                        OrderType? orderType = null;
                        if (order != null)
                        {
                            orderType = order.OrderType;
                        }

WriteLiteral("                        <th class=\"ui-state-default ");


                                                Write((orderType == null) ? "" : (orderType == OrderType.Ascending ? "sf-header-sort-down" : "sf-header-sort-up"));

WriteLiteral("\">\r\n                            <input type=\"hidden\" value=\"");


                                                   Write(col.Name);

WriteLiteral("\" />\r\n                            ");


                       Write(col.DisplayName);

WriteLiteral("\r\n                        </th>\r\n");


                    }

WriteLiteral("                </tr>\r\n            </thead>\r\n            <tbody class=\"ui-widget-" +
"content\">\r\n");


                   int columnsCount = columns.Count + (viewable ? 1 : 0) + (allowMultiple.HasValue ? 1 : 0); 

WriteLiteral("                <tr>\r\n                    <td colspan=\"");


                            Write(columnsCount);

WriteLiteral("\">");


                                           Write(Resources.Signum_noResults);

WriteLiteral("</td>\r\n                </tr>\r\n");


                   
                    ViewData[ViewDataKeys.ElementsPerPage] = findOptions.ElementsPerPage ?? (findOptions.ElementsPerPageEmpty ? null : Navigator.Manager.QuerySettings.GetOrThrow(findOptions.QueryName, "Missing QuerySettings for QueryName {0}").ElementsPerPage);
                    ViewData[ViewDataKeys.FilterMode] = findOptions.FilterMode;
                    ViewData[ViewDataKeys.SearchControlColumnsCount] = columnsCount;
                

WriteLiteral("                ");


           Write(Html.Partial(Navigator.Manager.PaginationView, Model));

WriteLiteral("\r\n            </tbody>\r\n        </table>\r\n    </div>\r\n</div>\r\n<script type=\"text/" +
"javascript\">\r\n    new SF.FindNavigator({ prefix: \"");


                               Write(Model.ControlID);

WriteLiteral("\" }).initialize();\r\n</script>\r\n");


        }
    }
}
