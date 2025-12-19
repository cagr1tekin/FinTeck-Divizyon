# Production Build Script (PowerShell)
# İnteraktif Kredi - Web Şube 2.0

Write-Host "🚀 Production Build Başlatılıyor..." -ForegroundColor Green

# 1. SCSS Derleme
Write-Host "`n📦 SCSS derleniyor ve minify ediliyor..." -ForegroundColor Yellow
npm run scss:build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ SCSS derleme başarısız!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ SCSS derleme tamamlandı" -ForegroundColor Green

# 2. .NET Build (Release)
Write-Host "`n🔨 .NET projesi Release modunda build ediliyor..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ .NET build başarısız!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET build tamamlandı" -ForegroundColor Green

# 3. .NET Publish
Write-Host "`n📤 .NET projesi publish ediliyor..." -ForegroundColor Yellow
$publishPath = "./publish"
if (Test-Path $publishPath) {
    Remove-Item -Path $publishPath -Recurse -Force
}
dotnet publish -c Release -o $publishPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ .NET publish başarısız!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET publish tamamlandı" -ForegroundColor Green

# 4. Dosya kontrolü
Write-Host "`n📋 Publish klasörü kontrol ediliyor..." -ForegroundColor Yellow
$requiredFiles = @(
    "$publishPath/wwwroot/css/main.css",
    "$publishPath/wwwroot/js/app.js",
    "$publishPath/InteraktifKredi.dll"
)

$allFilesExist = $true
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "  ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $file bulunamadı!" -ForegroundColor Red
        $allFilesExist = $false
    }
}

if (-not $allFilesExist) {
    Write-Host "`n❌ Bazı gerekli dosyalar eksik!" -ForegroundColor Red
    exit 1
}

# 5. Özet
Write-Host "`n✨ Production build tamamlandı!" -ForegroundColor Green
Write-Host "📁 Publish klasörü: $publishPath" -ForegroundColor Cyan
Write-Host "`n📝 Sonraki adımlar:" -ForegroundColor Yellow
Write-Host "  1. appsettings.Production.json dosyasını kontrol edin" -ForegroundColor White
Write-Host "  2. Environment variables'ları ayarlayın" -ForegroundColor White
Write-Host "  3. DEPLOYMENT_CHECKLIST.md dosyasını takip edin" -ForegroundColor White
Write-Host "`n🎉 Hazır!" -ForegroundColor Green

