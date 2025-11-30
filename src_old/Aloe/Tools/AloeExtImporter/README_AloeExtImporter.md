# Aloe External Importer

郵便番号、企業情報、MJ縮退マップ などのデータをダウンロードし、取り込むためのプロジェクトです。

対象のシステムの DB の ext スキーマ上に取り込み用テーブルと実テーブルを用意しておきます。
スキーマを分けておくことでバックアップしない選択が取れます。
取り込み用テーブルを用意しておくことで、CSV をそのまま取り込むことができます。

## 各種マスタ

### 住所

郵便番号と住所のCSVをダウンロードできます。
約13万件 20MB程度

日本郵便 郵便番号データダウンロード
https://www.post.japanpost.jp/zipcode/download.html


住所の郵便番号（1レコード1行、UTF-8形式）（CSV形式）
https://www.post.japanpost.jp/zipcode/dl/utf-zip.html
https://www.post.japanpost.jp/zipcode/dl/utf/zip/utf_ken_all.zip
utf_ken_all.zip 2MB程度

### 法人

基本3情報(名称、住所、法人番号)のCSVをダウンロードできます。
約600万件 1GB程度

JIS縮退マップのXLSXをダウンロードできます。
約12万件 1MB程度

国税庁 法人番号公表サイト
https://www.houjin-bangou.nta.go.jp/pc/index.html

基本３情報ダウンロード
https://www.houjin-bangou.nta.go.jp/download/

基本３情報ダウンロード &gt; 全件データのダウンロード &gt; CSV形式・Unicode
https://www.houjin-bangou.nta.go.jp/download/zenken/#csv-unicode

### MJ縮退マップ

MJ縮退マップのJSONをダウンロードできます。

縮退マップとは一般的に表示できない文字を表示できる文字に置き換えるための情報です。
約6万文字をJIS(第1水準～第4水準)へマッピングしています。

文字情報技術促進協議会 MJ縮退マップ
https://moji.or.jp/mojikiban/map/

※国税庁のJIS縮退マップを使用すれば、JIS第2水準までに変換できます。
それ以上が必要な場合に使用を検討します。

### 医薬品HOTコードマスター

医薬品HOTコードのCSVをダウンロードできます。
HOTコードマスター 約7万件 18MB程度

医薬品HOT コードマスター ダウンロード ZIP版
https://www2.medis.or.jp/hcode/

レセ用のコードが必要な場合は、支払基金の医薬品マスターを検討する。
https://www.ssk.or.jp/seikyushiharai/tensuhyo/kihonmasta/

FHIR HOT13
https://jpfhir.jp/fhir/core/terminology/igv1/CodeSystem-medis-codesystem-hot13.html

### 病名マスター

病名マスター(ICD10対応)のCSVをダウンロードできます。
病名マスター 約3万件 5MB程度
修飾語マスター 約3千件 50KB程度
索引語マスター 約12万件 7MB程度

ICD10で定められた国際標準の病名マスターに、
不足している情報を付け足す修飾語マスターと、
それらを検索するための索引語マスターです。

MEDIS ICD10対応標準病名マスター(CSV形式)
https://www2.medis.or.jp/stdcd/byomei/index.html

### 臨床検査マスター

臨床検査マスター(JLAC10)のEXCELをダウンロードできます。
約1万件 10MB程度

MEDIS 臨床検査マスター(EXCEL形式)
https://www2.medis.or.jp/master/kensa/index.html

※例えば、白血球数について、特定健診で要求される「2A010 白血球数」は個別項目であり、
臨床検査マスターでは「2A990 末梢血液一般検査」のセット項目に白血球数が含まれるため個別項目としては含まれていません。
また、特定健診の問診項目も含まれていません。

### XML用特定健診検査項目情報

XML用特定健診検査項目情報のEXCELをダウンロードできます。
約300件 100KB程度

厚生労働省 電子的な標準様式 第４期（2024年度～2029年度分）
https://www.mhlw.go.jp/stf/seisakunitsuite/bunya/xml_30799.html

※一部の個別項目は臨床検査マスターには記載されていません。

### FHIR 健康診断結果報告書

検診結果報告用の健診項目のJSONをダウンロードできます。

健康診断結果報告書のFHIR実装ガイド
https://jpfhir.jp/fhir/eCheckup/

健診結果　健診項目コード ValueSet
https://jpfhir.jp/fhir/eCheckup/igv1/ValueSet-jp-observationcode-vs.html

※一部の個別項目は臨床検査マスターには記載されていません。

※厚労省のXML用特定健診検査項目情報と同じ項目です。
JLAC10コードの一覧のみ欲しい場合に利用できます。

### 特定健診・特定保健指導の機関コード

特定健診・特定保健指導の機関コードCSVをダウンロード出来ます。
約6万件 10MB程度

社会保険診療報酬支払基金 機関情報一括ダウンロード
https://www.ssk.or.jp/kikankensaku/html/download.html

### JP FHIR

https://jpfhir.jp/fhir/core/terminology/igv1/CodeSystem-medis-codesystem-hot13.html



## 各種レイアウト

### 郵便番号CSVレイアウト

https://www.post.japanpost.jp/zipcode/dl/readme.html

1. 全国地方公共団体コード（JIS X0401、X0402）………　半角数字
2. （旧）郵便番号（5桁）………………………………………　半角数字
3. 郵便番号（7桁）………………………………………　半角数字
4. 都道府県名　…………　半角カタカナ（コード順に掲載）　（※1）
5. 市区町村名　…………　半角カタカナ（コード順に掲載）　（※1）
6. 町域名　………………　半角カタカナ（五十音順に掲載）　（※1）
7. 都道府県名　…………　漢字（コード順に掲載）　（※1,2）
8. 市区町村名　…………　漢字（コード順に掲載）　（※1,2）
9. 町域名　………………　漢字（五十音順に掲載）　（※1,2）
10. 一町域が二以上の郵便番号で表される場合の表示　（※3）　（「1」は該当、「0」は該当せず）
11. 小字毎に番地が起番されている町域の表示　（※4）　（「1」は該当、「0」は該当せず）
12. 丁目を有する町域の場合の表示　（「1」は該当、「0」は該当せず）
13. 一つの郵便番号で二以上の町域を表す場合の表示　（※5）　（「1」は該当、「0」は該当せず）
14. 更新の表示（※6）（「0」は変更なし、「1」は変更あり、「2」廃止（廃止データのみ使用））
15. 変更理由　（「0」は変更なし、「1」市政・区政・町政・分区・政令指定都市施行、「2」住居表示の実施、「3」区画整理、「4」郵便区調整等、「5」訂正、「6」廃止（廃止データのみ使用））

### 法人番号CSVレイアウト

https://www.houjin-bangou.nta.go.jp/pc/download/images/k-resource-dl.pdf

### JIS縮退マップのレイアウト

実ファイルより
https://www.houjin-bangou.nta.go.jp/pc/download/images/jissyukutaimap1_0_0.xlsx

変換元の文字（JISX0213：1-4水）
1. 面区点コード
2. Unicode
3. 字形
4. JIS区分

コード変換（1対1変換）
5. 面区点コード
6. Unicode
7. 字形

文字列変換（追加非漢字や、1対ｎの文字変換を行う）
※主に記号など
8. 面区点コード①
9. 面区点コード②
10. 面区点コード③
11. 面区点コード④
12. Unicode①
13. Unicode②
14. Unicode③
15. Unicode④
16. 字形
17. 備考

### 標準病名マスターのレイアウト

https://www2.medis.or.jp/stdcd/byomei/spc515.pdf

### 臨床検査マスター(17桁コード表)のレイアウト

新規登録分が最下部に途中にヘッダーありで挿入されている可能性があります。
取り込む際には途中のヘッダーを削除してから取り込んでください。

### XML用特定健診検査項目情報のレイアウト

最後に空白行が含まれることがあります。
取り込む際には除外してください。
また、一部の列は取り込んでいません。

### 特定健診・特定保健指導の機関コードのレイアウト

1. 機関コード
2. 機関種別
3. 機関名
4. 郵便番号
5. 電話番号
6. 機関所在地
7. ホームページ
8. 経営主体
