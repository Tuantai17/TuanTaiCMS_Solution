[xml]$xml = Get-Content -Path "e:\ASP.NET\TuanTaiCMS_Solution\docx_extracted\word\document.xml" -Encoding UTF8
$text = $xml.document.body.InnerText
[System.IO.File]::WriteAllText("e:\ASP.NET\TuanTaiCMS_Solution\docx_extracted_content.txt", $text, [System.Text.Encoding]::UTF8)
