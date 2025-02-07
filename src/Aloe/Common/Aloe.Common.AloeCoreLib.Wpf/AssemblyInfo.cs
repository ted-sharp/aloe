using System.Runtime.InteropServices;
using System.Windows.Markup;

// このような SDK スタイルのプロジェクトの場合、以前はこのファイルで定義していたいくつかのアセンブリ属性がビルド時に自動的に追加されて、プロジェクトのプロパティで定義されている値がそれに設定されるようになりました。組み込まれる属性と、このプロセスをカスタマイズする方法の詳細については、次を参照してください:
// https://aka.ms/assembly-info-properties


// ComVisible を false に設定すると、このアセンブリ内の型は COM コンポーネントから参照できなくなります。このアセンブリ内の型に COM からアクセスする必要がある場合は、その型の
// ComVisible 属性を true に設定してください。

[assembly: ComVisible(false)]

// このプロジェクトが COM に公開される場合、次の GUID が typelib の ID になります。

[assembly: Guid("7371fe94-5baa-4d24-97da-96d8cc0f2e5e")]

[assembly: XmlnsDefinition("http://schemas.aloe/wpf/behaviors", "Aloe.Common.AloeCoreLib.Wpf.Behaviors")]
[assembly: XmlnsPrefix("http://schemas.aloe/wpf/behaviors", "bhv")]

[assembly: XmlnsDefinition("http://schemas.aloe/wpf/converters", "Aloe.Common.AloeCoreLib.Wpf.Converters")]
[assembly: XmlnsPrefix("http://schemas.aloe/wpf/converters", "cvt")]

[assembly: XmlnsDefinition("http://schemas.aloe/wpf/extension", "Aloe.Common.AloeCoreLib.Wpf.Extension")]
[assembly: XmlnsPrefix("http://schemas.aloe/wpf/extension", "ext")]
