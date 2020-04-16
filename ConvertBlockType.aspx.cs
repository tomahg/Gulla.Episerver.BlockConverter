// Decompiled with JetBrains decompiler
// Type: EPiServer.UI.Admin.ConvertPageType
// Assembly: EPiServer.UI, Version=11.21.3.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7
// MVID: C6E15AF7-5923-47BA-AD64-EF118C5B5546
// Assembly location: EPiServer.UI.dll

using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using EPiServer.MirroringService;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using EPiServer.UI.WebControls;
using EPiServer.Web;
using EPiServer.Web.WebControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPiServer.UI.Admin
{
  public class ConvertPageType : WebFormsBase
  {
    private const string TEST_BUTTON_ID = "TestButton";
    private const string VS_SELECTED_PAGELINK = "SelectedPageLink";
    protected InputPageReference PageRoot;
    protected CheckBox Recursive;
    protected ConvertPageTypeProperties Properties;
    protected ToolButton ConvertButton;
    protected ToolButton TestButton;

    protected Injected<IPageCriteriaQueryService> QueryService { get; set; }

    internal Injected<IPermanentLinkMapper> PermanentLinkMapper { get; set; }

    internal Injected<IContentLoader> ContentLoader { get; set; }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if (this.IsPostBack)
        return;
      this.DataBind();
    }

    protected string ConfirmMessage
    {
      get
      {
        return string.Format("{0}\\n{1}\\n{2}", (object) this.Translate("/admin/convertpagetype/removepropertywarningheading"), (object) this.Translate("/admin/convertpagetype/removepropertywarning1"), (object) this.Translate("/admin/convertpagetype/removepropertywarning2"));
      }
    }

    private bool VerifyFormsValues(bool isTest)
    {
      if (PageReference.IsNullOrEmpty(this.PageRoot.PageLink))
        throw new EPiServerException(this.Translate("/admin/convertpagetype/noselectedpage"));
      if (this.PageRoot.PageLink.IsExternalProvider)
        throw new EPiServerException(this.Translate("/admin/convertpagetype/remotepage"));
      PageData page = this.ContentRepository.Get<PageData>((ContentReference) this.PageRoot.PageLink);
      PageDataCollection pageDataCollection = new PageDataCollection();
      if (page.ContentTypeID == this.Properties.FromPageType.ID)
        pageDataCollection.Add(page);
      if (!this.Recursive.Checked && pageDataCollection.Count == 0)
        throw new EPiServerException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, this.Translate("/admin/convertpagetype/notmatchingpagetype"), (object) page.PageTypeName));
      if (pageDataCollection.Count == 0)
      {
        if (this.QueryService.Service.FindPagesWithCriteria(page.PageLink, new PropertyCriteriaCollection()
        {
          new PropertyCriteria()
          {
            Condition = CompareCondition.Equal,
            Name = "PageTypeID",
            Type = PropertyDataType.PageType,
            Value = this.Properties.FromPageType.ID.ToString()
          }
        }).Count == 0)
          throw new EPiServerException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, this.Translate("/admin/convertpagetype/nomatchingpages"), (object) page.PageTypeName));
      }
      return true;
    }

    protected void Convert(object sender, EventArgs e)
    {
      bool isTest = ((Control) sender).ID.Equals("TestButton");
      try
      {
        if (isTest)
        {
          SystemMessageContainer messageContainer = this.SystemMessageContainer;
          messageContainer.Message = messageContainer.Message + "<strong>" + this.Translate("/admin/convertpagetype/istesting") + "</strong><br/>";
        }
        this.VerifyFormsValues(isTest);
        this.SystemMessageContainer.Message += HttpUtility.HtmlEncode(PageTypeConverter.Convert(this.PageRoot.PageLink, this.Properties.FromPageType as PageType, this.Properties.ToPageType as PageType, this.Properties.GetMappingsForProperties(), this.Recursive.Checked, isTest)).Replace("\n", "<br/>");
        foreach (string str in (IEnumerable<string>) this.GetEffectedMirorringChannelsByConvertingPage(this.PageRoot.PageLink, this.Recursive.Checked))
        {
          SystemMessageContainer messageContainer = this.SystemMessageContainer;
          messageContainer.Message = messageContainer.Message + "<br/>" + string.Format((IFormatProvider) CultureInfo.InvariantCulture, this.TranslateFallback("/admin/convertpagetype/haseffectonmirroring", "Convert page has effect on mirroring channel {0}."), (object) str);
        }
      }
      catch (EPiServerException ex)
      {
        SystemMessageContainer messageContainer = this.SystemMessageContainer;
        messageContainer.Message = messageContainer.Message + this.Translate("/admin/convertpagetype/converterror") + ex.Message;
      }
      catch (Exception ex)
      {
        SystemMessageContainer messageContainer = this.SystemMessageContainer;
        messageContainer.Message = messageContainer.Message + this.Translate("/admin/convertpagetype/converterror") + ex.Message + "\n" + ex.StackTrace.Replace("\n", "<br/>");
      }
    }

    private IList<string> GetEffectedMirorringChannelsByConvertingPage(
      PageReference convertPageRef,
      bool recursive)
    {
      List<string> stringList = new List<string>();
      foreach (MirroringData mirroringData in (IEnumerable<MirroringData>) MirroringData.List())
      {
        if (mirroringData.InitialMirroringDone)
        {
          PermanentLinkMap permanentLinkMap = this.PermanentLinkMapper.Service.Find(mirroringData.FromPageGuid);
          if (permanentLinkMap != null && !ContentReference.IsNullOrEmpty(permanentLinkMap.ContentReference))
          {
            if (this.ContentRepository.GetAncestors((ContentReference) convertPageRef).Select<IContent, ContentReference>((Func<IContent, ContentReference>) (p => p.ContentLink)).Contains<ContentReference>(permanentLinkMap.ContentReference) || permanentLinkMap.ContentReference.CompareToIgnoreWorkID((ContentReference) convertPageRef))
              stringList.Add(mirroringData.Name);
            else if (recursive && this.ContentRepository.GetAncestors(permanentLinkMap.ContentReference).Select<IContent, ContentReference>((Func<IContent, ContentReference>) (p => p.ContentLink)).Contains<ContentReference>((ContentReference) convertPageRef))
              stringList.Add(mirroringData.Name);
          }
        }
      }
      return (IList<string>) stringList;
    }
  }
}
