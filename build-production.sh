#!/bin/bash
# Production Build Script (Bash)
# İnteraktif Kredi - Web Şube 2.0

echo "🚀 Production Build Başlatılıyor..."

# 1. SCSS Derleme
echo ""
echo "📦 SCSS derleniyor ve minify ediliyor..."
npm run scss:build
if [ $? -ne 0 ]; then
    echo "❌ SCSS derleme başarısız!"
    exit 1
fi
echo "✅ SCSS derleme tamamlandı"

# 2. .NET Build (Release)
echo ""
echo "🔨 .NET projesi Release modunda build ediliyor..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "❌ .NET build başarısız!"
    exit 1
fi
echo "✅ .NET build tamamlandı"

# 3. .NET Publish
echo ""
echo "📤 .NET projesi publish ediliyor..."
PUBLISH_PATH="./publish"
if [ -d "$PUBLISH_PATH" ]; then
    rm -rf "$PUBLISH_PATH"
fi
dotnet publish -c Release -o "$PUBLISH_PATH"
if [ $? -ne 0 ]; then
    echo "❌ .NET publish başarısız!"
    exit 1
fi
echo "✅ .NET publish tamamlandı"

# 4. Dosya kontrolü
echo ""
echo "📋 Publish klasörü kontrol ediliyor..."
REQUIRED_FILES=(
    "$PUBLISH_PATH/wwwroot/css/main.css"
    "$PUBLISH_PATH/wwwroot/js/app.js"
    "$PUBLISH_PATH/InteraktifKredi.dll"
)

ALL_FILES_EXIST=true
for file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "  ✅ $file"
    else
        echo "  ❌ $file bulunamadı!"
        ALL_FILES_EXIST=false
    fi
done

if [ "$ALL_FILES_EXIST" = false ]; then
    echo ""
    echo "❌ Bazı gerekli dosyalar eksik!"
    exit 1
fi

# 5. Özet
echo ""
echo "✨ Production build tamamlandı!"
echo "📁 Publish klasörü: $PUBLISH_PATH"
echo ""
echo "📝 Sonraki adımlar:"
echo "  1. appsettings.Production.json dosyasını kontrol edin"
echo "  2. Environment variables'ları ayarlayın"
echo "  3. DEPLOYMENT_CHECKLIST.md dosyasını takip edin"
echo ""
echo "🎉 Hazır!"

