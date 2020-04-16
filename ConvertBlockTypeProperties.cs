// Decompiled with JetBrains decompiler
// Type: EPiServer.UI.WebControls.ConvertPageTypeProperties
// Assembly: EPiServer.UI, Version=11.21.3.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7
// MVID: C6E15AF7-5923-47BA-AD64-EF118C5B5546
// Assembly location: EPiServer.UI.dll

using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPiServer.UI.WebControls
{
    public class ConvertPageTypeProperties : Control, INamingContainer
    {
        private List<DropDownList> _properties = new List<DropDownList>();
        private const string FROM_PAGE_TYPE_ID = "From";
        private const string TO_PAGE_TYPE_ID = "To";
        private const string VS_TO_TYPE = "toType";
        private const string VS_FROM_TYPE = "fromType";
        private const string VS_LANGUAGE = "language";
        private IContentTypeRepository _contentTypeRepository;
        private IContentLoader _contentLoader;

        public ConvertPageTypeProperties()
        {
            this._contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            this._contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        }

        protected override void CreateChildControls()
        {
            Table table1 = new Table();
            table1.CssClass = "epi-default";
            table1.Rows.Add(this.CreatePageTypeHeaderRow());
            table1.Rows.Add(this.CreatePageTypeRow(this.FromPageType, this.ToPageType));
            this._properties.Clear();
            this.Controls.Add((Control)table1);
            Table table2 = new Table();
            table2.CssClass = "epi-default";
            table2.Rows.Add(this.CreatePropertyHeaderRow());
            if (this.FromPageType != (ContentType)null)
            {
                foreach (PropertyDefinition propertyDefinition in this.FromPageType.PropertyDefinitions)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell()
                    {
                        Text = propertyDefinition.Name
                    });
                    TableCell cell = new TableCell();
                    cell.Controls.Add((Control)this.CreatePropertyDropDown(propertyDefinition, this.ToPageType));
                    row.Cells.Add(cell);
                    table2.Rows.Add(row);
                }
            }
            this.Controls.Add((Control)table2);
        }

        private TableRow CreatePageTypeHeaderRow()
        {
            TableRow tableRow = new TableRow();
            TableCell cell1 = (TableCell)new TableHeaderCell();
            cell1.Text = LocalizationService.Current.GetString("/admin/convertpagetype/frompagetype");
            tableRow.Cells.Add(cell1);
            TableCell cell2 = (TableCell)new TableHeaderCell();
            cell2.Text = LocalizationService.Current.GetString("/admin/convertpagetype/topagetype");
            tableRow.Cells.Add(cell2);
            return tableRow;
        }

        private TableCell CreatePageTypeCell(
          string ID,
          ContentType selectedPageType,
          ContentType disabledPageType)
        {
            TableCell tableCell = new TableCell();
            DropDownList dropDownList = new DropDownList();
            dropDownList.ID = ID;
            dropDownList.DataSource = (object)this._contentTypeRepository.List().OfType<PageType>().Where<PageType>((Func<PageType, bool>)(pageType => pageType.ID != this._contentLoader.Get<PageData>((ContentReference)ContentReference.RootPage).ContentTypeID && pageType.ID != this._contentLoader.Get<PageData>((ContentReference)ContentReference.WasteBasket).ContentTypeID));
            dropDownList.AutoPostBack = true;
            dropDownList.DataTextField = "LocalizedName";
            dropDownList.DataValueField = nameof(ID);
            dropDownList.SelectedIndexChanged += new EventHandler(this.PageTypeChanged);
            tableCell.Style.Add("border-bottom", "none");
            tableCell.Controls.Add((Control)dropDownList);
            tableCell.DataBind();
            int id;
            if (disabledPageType != (ContentType)null)
            {
                ListItemCollection items = dropDownList.Items;
                id = disabledPageType.ID;
                string str = id.ToString();
                ListItem byValue = items.FindByValue(str);
                if (byValue != null)
                    dropDownList.Items.Remove(byValue);
            }
            if (!(selectedPageType != (ContentType)null))
                return tableCell;
            ListItemCollection items1 = dropDownList.Items;
            id = selectedPageType.ID;
            string str1 = id.ToString();
            ListItem byValue1 = items1.FindByValue(str1);
            if (byValue1 == null)
                return tableCell;
            byValue1.Selected = true;
            return tableCell;
        }

        private TableRow CreatePageTypeRow(ContentType fromPageType, ContentType toPageType)
        {
            TableRow tableRow = new TableRow();
            tableRow.Style.Add("border-bottom", "none");
            tableRow.Cells.Add(this.CreatePageTypeCell("From", fromPageType, (ContentType)null));
            tableRow.Cells.Add(this.CreatePageTypeCell("To", toPageType, fromPageType));
            return tableRow;
        }

        private TableRow CreatePropertyHeaderRow()
        {
            TableRow tableRow = new TableRow();
            TableCell cell1 = (TableCell)new TableHeaderCell();
            cell1.Text = LocalizationService.Current.GetString("/admin/convertpagetype/frompageproperties");
            tableRow.Cells.Add(cell1);
            TableCell cell2 = (TableCell)new TableHeaderCell();
            cell2.Text = LocalizationService.Current.GetString("/admin/convertpagetype/topageproperties");
            tableRow.Cells.Add(cell2);
            return tableRow;
        }

        private DropDownList CreatePropertyDropDown(
          PropertyDefinition fromPropertyType,
          ContentType toPageType)
        {
            bool flag = false;
            DropDownList dropDownList = new DropDownList();
            dropDownList.ID = fromPropertyType.ID.ToString((IFormatProvider)CultureInfo.InvariantCulture);
            foreach (PropertyDefinition propertyDefinition in toPageType.PropertyDefinitions)
            {
                if (propertyDefinition.Type.DataType == fromPropertyType.Type.DataType)
                {
                    BlockPropertyDefinitionType type1 = fromPropertyType.Type as BlockPropertyDefinitionType;
                    BlockPropertyDefinitionType type2 = propertyDefinition.Type as BlockPropertyDefinitionType;
                    if (type1 == null || type2 == null || !(type1.BlockType.GUID != type2.BlockType.GUID))
                    {
                        ListItem listItem = new ListItem(propertyDefinition.Name, propertyDefinition.ID.ToString((IFormatProvider)CultureInfo.InvariantCulture));
                        if (!flag && string.Compare(propertyDefinition.Name, fromPropertyType.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                            flag = listItem.Selected = true;
                        dropDownList.Items.Add(listItem);
                    }
                }
            }
            ListItem listItem1 = new ListItem(LocalizationService.Current.GetString("/admin/convertpagetype/removepropety"), "");
            if (!flag)
                listItem1.Selected = true;
            dropDownList.Items.Add(listItem1);
            this._properties.Add(dropDownList);
            return dropDownList;
        }

        public List<KeyValuePair<int, int>> GetMappingsForProperties()
        {
            List<KeyValuePair<int, int>> keyValuePairList = new List<KeyValuePair<int, int>>();
            List<int> intList = new List<int>();
            foreach (DropDownList property in this._properties)
            {
                if (!string.IsNullOrEmpty(property.SelectedItem.Value))
                {
                    if (intList.Contains(int.Parse(property.SelectedItem.Value)))
                        throw new EPiServerException(LocalizationService.Current.GetString("/admin/convertpagetype/converterrorsameproperty"));
                    intList.Add(int.Parse(property.SelectedItem.Value));
                    KeyValuePair<int, int> keyValuePair = new KeyValuePair<int, int>(int.Parse(property.ID), int.Parse(property.SelectedItem.Value));
                    keyValuePairList.Insert(0, keyValuePair);
                }
                else
                {
                    KeyValuePair<int, int> keyValuePair = new KeyValuePair<int, int>(int.Parse(property.ID), 0);
                    keyValuePairList.Add(keyValuePair);
                }
            }
            return keyValuePairList;
        }

        public ContentType FromPageType
        {
            get
            {
                return this.ViewState["fromType"] != null ? this._contentTypeRepository.Load((int)this.ViewState["fromType"]) : this._contentTypeRepository.List().Where<ContentType>((Func<ContentType, bool>)(pageType => pageType.ID != this._contentLoader.Get<PageData>((ContentReference)ContentReference.RootPage).ContentTypeID && pageType.ID != this._contentLoader.Get<PageData>((ContentReference)ContentReference.WasteBasket).ContentTypeID)).FirstOrDefault<ContentType>();
            }
            set
            {
                if (value != (ContentType)null)
                    this.ViewState["fromType"] = (object)value.ID;
                else
                    this.ViewState["fromType"] = (object)null;
            }
        }

        public ContentType ToPageType
        {
            get
            {
                if (this.ViewState["toType"] != null)
                {
                    ContentType contentType = this._contentTypeRepository.Load((int)this.ViewState["toType"]);
                    if (contentType != (ContentType)null && (this.FromPageType == (ContentType)null || !this.FromPageType.Equals((object)contentType)))
                        return contentType;
                }
                List<ContentType> list = this._contentTypeRepository.List().Where<ContentType>((Func<ContentType, bool>)(pageType => pageType.ID != this._contentLoader.Get<PageData>((ContentReference)ContentReference.RootPage).ContentTypeID && pageType.ID != this._contentLoader.Get<PageData>((ContentReference)ContentReference.WasteBasket).ContentTypeID)).ToList<ContentType>();
                if (this.FromPageType == (ContentType)null || list.Count < 2)
                    return (ContentType)null;
                return !list[1].Equals((object)this.FromPageType) ? list[1] : list[0];
            }
            set
            {
                if (value != (ContentType)null)
                    this.ViewState["toType"] = (object)value.ID;
                else
                    this.ViewState["toType"] = (object)null;
            }
        }

        private void PageTypeChanged(object sender, EventArgs e)
        {
            DropDownList dropDownList = (DropDownList)sender;
            string selectedValue = dropDownList.SelectedValue;
            if (!string.IsNullOrEmpty(selectedValue))
            {
                if (dropDownList.ID.Equals("From"))
                    this.FromPageType = this._contentTypeRepository.Load(int.Parse(selectedValue));
                else
                    this.ToPageType = this._contentTypeRepository.Load(int.Parse(selectedValue));
            }
            this.ClearChildViewState();
            this.Controls.Clear();
            this.CreateChildControls();
        }
    }
}
