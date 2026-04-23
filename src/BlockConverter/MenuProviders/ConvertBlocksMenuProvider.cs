using EPiServer.Shell.Navigation;

namespace Gulla.Episerver.BlockConverter.MenuProviders;

[MenuProvider]
public class ConvertBlocksMenuProvider : IMenuProvider
{
    public IEnumerable<MenuItem> GetMenuItems()
    {
        yield return new UrlMenuItem(
            "Convert Blocks",
            MenuPaths.Global + "/cms/admin/tools/convertblocks",
            "/convertblocks")
        {
            SortIndex = 53
        };
    }
}
