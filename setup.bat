# Klasörü oluştur ve içine gir
mkdir FastCart
cd FastCart

# Çözüm dosyasını oluştur
dotnet new sln -n FastCart

# Katmanları oluştur
dotnet new webapi -n FastCart.API
dotnet new classlib -n FastCart.Application
dotnet new classlib -n FastCart.Infrastructure
dotnet new classlib -n FastCart.Domain
dotnet new console -n FastCart.Console    

# Çözüm dosyasına projeleri ekle
dotnet sln add FastCart.API/FastCart.API.csproj
dotnet sln add FastCart.Application/FastCart.Application.csproj
dotnet sln add FastCart.Infrastructure/FastCart.Infrastructure.csproj
dotnet sln add FastCart.Domain/FastCart.Domain.csproj
dotnet sln add FastCart.Console/FastCart.Console.csproj 

# Katmanlar arası referansları ayarla
dotnet add FastCart.API reference FastCart.Application
dotnet add FastCart.Application reference FastCart.Domain
dotnet add FastCart.Application reference FastCart.Infrastructure
dotnet add FastCart.Infrastructure reference FastCart.Domain
dotnet add FastCart.Console reference FastCart.Application
dotnet add FastCart.Console reference FastCart.Infrastructure
dotnet add FastCart.Console reference FastCart.Domain

# ----------------------------------------
# ✅ Test projesi isteğe bağlı olarak sonradan eklenir
# ----------------------------------------
echo ""
echo "🎯 Test projesi eklemek için aşağıdaki komutları çalıştır:"
echo "dotnet new xunit -n FastCart.Tests.Unit"
echo "dotnet sln add FastCart.Tests.Unit/FastCart.Tests.Unit.csproj"
echo "dotnet add FastCart.Tests.Unit reference FastCart.Application"
echo "dotnet add FastCart.Tests.Unit reference FastCart.Infrastructure"
echo "dotnet add FastCart.Tests.Unit reference FastCart.Domain"

