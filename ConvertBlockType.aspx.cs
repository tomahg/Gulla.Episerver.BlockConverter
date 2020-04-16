using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.MirroringService;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using EPiServer.UI.WebControls;
using EPiServer.Web;

namespace Alloy.Business.ConvertBlocks
{
    public class ConvertBlockType : WebFormsBase
    {
        private const string TEST_BUTTON_ID = "TestButton";
        private const string VS_SELECTED_PAGELINK = "SelectedPageLink";
        protected TextBox PageRoot;
        protected CheckBox Recursive;
        protected ConvertBlockTypeProperties Properties;
        protected ToolButton ConvertButton;
        protected ToolButton TestButton;

        internal Injected<IPermanentLinkMapper> PermanentLinkMapper { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (IsPostBack)
                return;
            DataBind();
        }

        protected string ConfirmMessage
        {
            get
            {
                return string.Format("{0}\\n{1}\\n{2}", Translate("/admin/convertblocktype/removepropertywarningheading"), Translate("/admin/convertblocktype/removepropertywarning1"), Translate("/admin/convertblocktype/removepropertywarning2"));
            }
        }

        private bool VerifyFormsValues(bool isTest)
        {
            if (string.IsNullOrEmpty(PageRoot.Text) && !Recursive.Checked)
                throw new EPiServerException("Specify block id, or convert ALL blocks of specified type.");

            return true;
        }

        protected void Convert(object sender, EventArgs e)
        {
            bool isTest = ((Control)sender).ID.Equals("TestButton");
            try
            {
                IContent content = null;

                if (!Recursive.Checked)
                {
                    content = (IContent)ContentRepository.Get<BlockData>(new ContentReference(int.Parse(PageRoot.Text)));
                }
                else
                {
                    content = ContentRepository.Get<PageData>(ContentReference.RootPage);
                }

                if (isTest)
                {
                    SystemMessageContainer messageContainer = SystemMessageContainer;
                    messageContainer.Message = messageContainer.Message + "<strong>" + Translate("/admin/convertblocktype/istesting") + "</strong><br/>";
                }
                VerifyFormsValues(isTest);
                SystemMessageContainer.Message += HttpUtility.HtmlEncode(BlockTypeConverter.Convert(content.ContentLink, Properties.FromBlockType as BlockType, Properties.ToBlockType as BlockType, Properties.GetMappingsForProperties(), Recursive.Checked, isTest)).Replace("\n", "<br/>");
                foreach (string str in GetEffectedMirorringChannelsByConvertingPage(content.ContentLink, Recursive.Checked))
                {
                    SystemMessageContainer messageContainer = SystemMessageContainer;
                    messageContainer.Message = messageContainer.Message + "<br/>" + string.Format(CultureInfo.InvariantCulture, TranslateFallback("/admin/convertblocktype/haseffectonmirroring", "Convert page has effect on mirroring channel {0}."), str);
                }
            }
            catch (EPiServerException ex)
            {
                SystemMessageContainer messageContainer = SystemMessageContainer;
                messageContainer.Message = messageContainer.Message + Translate("/admin/convertblocktype/converterror") + ex.Message;
            }
            catch (Exception ex)
            {
                SystemMessageContainer messageContainer = SystemMessageContainer;
                messageContainer.Message = messageContainer.Message + Translate("/admin/convertblocktype/converterror") + ex.Message + "\n" + ex.StackTrace.Replace("\n", "<br/>");
            }
        }

        private IList<string> GetEffectedMirorringChannelsByConvertingPage(
          ContentReference convertContentRef,
          bool recursive)
        {
            List<string> stringList = new List<string>();
            foreach (MirroringData mirroringData in MirroringData.List())
            {
                if (mirroringData.InitialMirroringDone)
                {
                    PermanentLinkMap permanentLinkMap = PermanentLinkMapper.Service.Find(mirroringData.FromPageGuid);
                    if (permanentLinkMap != null && !ContentReference.IsNullOrEmpty(permanentLinkMap.ContentReference))
                    {
                        if (ContentRepository.GetAncestors(convertContentRef).Select(p => p.ContentLink).Contains(permanentLinkMap.ContentReference) || permanentLinkMap.ContentReference.CompareToIgnoreWorkID(convertContentRef))
                            stringList.Add(mirroringData.Name);
                        else if (recursive && ContentRepository.GetAncestors(permanentLinkMap.ContentReference).Select(p => p.ContentLink).Contains(convertContentRef))
                            stringList.Add(mirroringData.Name);
                    }
                }
            }
            return stringList;
        }
    }
}
