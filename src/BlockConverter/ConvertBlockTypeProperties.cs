using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;

namespace Gulla.Episerver.BlockConverter
{
    public class ConvertBlockTypeProperties : Control, INamingContainer
    {
        private readonly List<DropDownList> _properties = new List<DropDownList>();
        private const string FromBlockTypeId = "From";
        private const string ToBlockTypeId = "To";
        private const string VsToType = "toType";
        private const string VsFromType = "fromType";
        private readonly IContentTypeRepository _contentTypeRepository;

        private DropDownList _ddlTo;

        public ConvertBlockTypeProperties()
        {
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        }

        protected override void CreateChildControls()
        {
            Table table1 = new Table();
            table1.CssClass = "epi-default";
            table1.Rows.Add(CreatePageTypeHeaderRow());
            table1.Rows.Add(CreateBlockTypeRow(FromBlockType, ToBlockType));
            _properties.Clear();
            Controls.Add(table1);
            Table table2 = new Table();
            table2.CssClass = "epi-default";
            table2.Rows.Add(CreatePropertyHeaderRow());
            if (FromBlockType != null)
            {
                foreach (PropertyDefinition propertyDefinition in FromBlockType.PropertyDefinitions)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell
                    {
                        Text = propertyDefinition.Name
                    });
                    TableCell cell = new TableCell();
                    cell.Controls.Add(CreatePropertyDropDown(propertyDefinition, ToBlockType));
                    row.Cells.Add(cell);
                    table2.Rows.Add(row);
                }
            }
            Controls.Add(table2);
        }

        private TableRow CreatePageTypeHeaderRow()
        {
            TableRow tableRow = new TableRow();
            TableCell cell1 = new TableHeaderCell();
            cell1.Text = LocalizationService.Current.GetString("/admin/convertblocktype/frompagetype");
            tableRow.Cells.Add(cell1);
            TableCell cell2 = new TableHeaderCell();
            cell2.Text = LocalizationService.Current.GetString("/admin/convertblocktype/topagetype");
            tableRow.Cells.Add(cell2);
            return tableRow;
        }

        private TableCell CreateBlockTypeCell(
          string controlId,
          ContentType selectedPageType,
          ContentType disabledPageType)
        {
            TableCell tableCell = new TableCell();
            DropDownList dropDownList = new DropDownList();
            dropDownList.ID = controlId;
            dropDownList.DataSource = _contentTypeRepository.List().OfType<BlockType>();
            dropDownList.AutoPostBack = true;
            dropDownList.DataTextField = "LocalizedName";
            dropDownList.DataValueField = nameof(ID);
            dropDownList.SelectedIndexChanged += BlockTypeChanged;
            if (controlId == ToBlockTypeId)
                _ddlTo = dropDownList;

            tableCell.Style.Add("border-bottom", "none");
            tableCell.Controls.Add(dropDownList);
            tableCell.DataBind();

            int id;
            if (disabledPageType != null)
            {
                ListItemCollection items = dropDownList.Items;
                id = disabledPageType.ID;
                string str = id.ToString();
                ListItem byValue = items.FindByValue(str);
                if (byValue != null)
                    dropDownList.Items.Remove(byValue);
            }
            if (!(selectedPageType != null))
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

        private TableRow CreateBlockTypeRow(ContentType fromBlockType, ContentType toBlockType)
        {
            TableRow tableRow = new TableRow();
            tableRow.Style.Add("border-bottom", "none");
            tableRow.Cells.Add(CreateBlockTypeCell(FromBlockTypeId, fromBlockType, null));
            tableRow.Cells.Add(CreateBlockTypeCell(ToBlockTypeId, toBlockType, null));
            return tableRow;
        }

        private TableRow CreatePropertyHeaderRow()
        {
            TableRow tableRow = new TableRow();
            TableCell cell1 = new TableHeaderCell();
            cell1.Text = LocalizationService.Current.GetString("/admin/convertblocktype/frompageproperties");
            tableRow.Cells.Add(cell1);
            TableCell cell2 = new TableHeaderCell();
            cell2.Text = LocalizationService.Current.GetString("/admin/convertblocktype/topageproperties");
            tableRow.Cells.Add(cell2);
            return tableRow;
        }

        private DropDownList CreatePropertyDropDown(
          PropertyDefinition fromPropertyType,
          ContentType toPageType)
        {
            bool flag = false;
            DropDownList dropDownList = new DropDownList();
            dropDownList.ID = fromPropertyType.ID.ToString(CultureInfo.InvariantCulture);
            foreach (PropertyDefinition propertyDefinition in toPageType.PropertyDefinitions)
            {
                if (propertyDefinition.Type.DataType == fromPropertyType.Type.DataType)
                {
                    BlockPropertyDefinitionType type1 = fromPropertyType.Type as BlockPropertyDefinitionType;
                    BlockPropertyDefinitionType type2 = propertyDefinition.Type as BlockPropertyDefinitionType;
                    if (type1 == null || type2 == null || !(type1.BlockType.GUID != type2.BlockType.GUID))
                    {
                        ListItem listItem = new ListItem(propertyDefinition.Name, propertyDefinition.ID.ToString(CultureInfo.InvariantCulture));
                        if (!flag && string.Compare(propertyDefinition.Name, fromPropertyType.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                            flag = listItem.Selected = true;
                        dropDownList.Items.Add(listItem);
                    }
                }
            }
            ListItem listItem1 = new ListItem(LocalizationService.Current.GetString("/admin/convertblocktype/removepropety"), "");
            if (!flag)
                listItem1.Selected = true;
            dropDownList.Items.Add(listItem1);
            _properties.Add(dropDownList);
            return dropDownList;
        }

        public List<KeyValuePair<int, int>> GetMappingsForProperties()
        {
            List<KeyValuePair<int, int>> keyValuePairList = new List<KeyValuePair<int, int>>();
            List<int> intList = new List<int>();
            foreach (DropDownList property in _properties)
            {
                if (!string.IsNullOrEmpty(property.SelectedItem.Value))
                {
                    if (intList.Contains(int.Parse(property.SelectedItem.Value)))
                        throw new EPiServerException(LocalizationService.Current.GetString("/admin/convertblocktype/converterrorsameproperty"));
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

        public ContentType FromBlockType
        {
            get
            {
                return ViewState[VsFromType] != null ? _contentTypeRepository.Load((int)ViewState[VsFromType]) : _contentTypeRepository.List().OfType<BlockType>().FirstOrDefault();
            }
            set
            {
                if (value != null)
                    ViewState[VsFromType] = value.ID;
                else
                    ViewState[VsFromType] = null;
            }
        }

        public ContentType ToBlockType
        {
            get
            {
                return ViewState[VsToType] != null ? _contentTypeRepository.Load((int)ViewState[VsToType]) : _contentTypeRepository.List().OfType<BlockType>().FirstOrDefault();
            }
            set
            {
                if (value != null)
                    ViewState[VsToType] = value.ID;
                else
                    ViewState[VsToType] = null;
            }
        }

        private void BlockTypeChanged(object sender, EventArgs e)
        {
            DropDownList dropDownList = (DropDownList)sender;
            string selectedValue = dropDownList.SelectedValue;
            if (!string.IsNullOrEmpty(selectedValue))
            {
                if (dropDownList.ID.Equals(FromBlockTypeId))
                {
                    FromBlockType = _contentTypeRepository.Load(int.Parse(selectedValue));
                }
                else
                {
                    ToBlockType = _contentTypeRepository.Load(int.Parse(selectedValue));
                }

                if (dropDownList.ID.Equals(ToBlockTypeId) || _ddlTo.SelectedValue == ToBlockType.ID.ToString())
                {
                    ClearChildViewState();
                    Controls.Clear();
                    CreateChildControls();
                }
            }
        }
    }
}
