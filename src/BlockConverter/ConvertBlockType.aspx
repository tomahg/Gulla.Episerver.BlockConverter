<%@ Page Language="c#" EnableViewState="true" CodeBehind="ConvertBlockType.aspx.cs" AutoEventWireup="False" Inherits="Gulla.Episerver.BlockConverter.ConvertBlockType" Title="ConvertBlockType" %>

<%@ Register TagPrefix="Gulla" Namespace="Gulla.Episerver.BlockConverter" Assembly="BlockConverter" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>

<asp:Content ContentPlaceHolderID="HeaderContentRegion" runat="server">
    <script type="text/javascript">
        // <![CDATA[

        function ShowDialog() {
            return confirm("<%=ConfirmMessage%>");
        }
// ]]>
    </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainRegion" runat="server">
    <div class="epi-formArea">
        <div style="border: solid 1px; padding: 0.2em; margin-bottom: 1em;">
            <strong>Experimental module - use at your own risk!</strong>
        </div>
        <div class="epi-size15">
            <div>
                <asp:Label runat="server" AssociatedControlID="PageRoot" Text="Select single block to convert (id)" />
                <asp:TextBox id="PageRoot" runat="server" />
            </div>
            <div class="epi-indent epi-size40">
                <asp:CheckBox ID="Recursive" Runat="server" />
                <asp:Label runat="server" AssociatedControlID="Recursive" Text="Convert all blocks (of the selected block type)" />
            </div>
        </div>
    </div>
    <br />
    <Gulla:ConvertBlockTypeProperties ID="Properties" Runat="server" />
    <div class="epi-buttonContainer">
        <EPiServerUI:ToolButton runat="server" id="ConvertButton" OnClientClick="return ShowDialog();" OnClick="Convert" ToolTip="<%$ Resources: EPiServer, admin.convertpagetype.convert %>" Text="<%$ Resources: EPiServer, admin.convertpagetype.convert %>" />
        <EPiServerUI:ToolButton runat="server" id="TestButton" OnClick="Convert" ToolTip="<%$ Resources: EPiServer, admin.convertpagetype.runtest %>" Text="<%$ Resources: EPiServer, admin.convertpagetype.runtest %>" />
    </div>
</asp:Content>
