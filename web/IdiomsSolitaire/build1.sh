dotnet restore IdiomsSolitaire.Blazor/IdiomsSolitaire.Blazor.csproj

dotnet publish IdiomsSolitaire.Blazor/IdiomsSolitaire.Blazor.csproj -c Release -o /usr/app

export db='server=localhost;port=3306;user=root;password=Password01!;database=test;'

# nohup dotnet IdiomsSolitaire.Blazor.dll &