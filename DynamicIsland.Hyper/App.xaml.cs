// 别名固定指向 WPF 的 Application，避免与 System.Windows.Forms.Application 歧义（CS0104）
using Application = System.Windows.Application;

namespace DynamicIsland.Hyper;

public partial class App : Application
{
}
