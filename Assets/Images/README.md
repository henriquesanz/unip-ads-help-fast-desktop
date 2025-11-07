# Pasta de Imagens

Esta pasta é destinada para armazenar imagens e recursos visuais da aplicação.

## Estrutura Recomendada:

- **logo.png** ou **logo.jpg** - Logo principal da aplicação
- **icon.ico** - Ícone da aplicação (para a barra de tarefas)
- Outras imagens conforme necessário

## Como usar no código:

### No XAML:
```xml
<Image Source="pack://application:,,,/Assets/Images/logo.png" />
```

### No código C#:
```csharp
var logoUri = new Uri("pack://application:,,,/Assets/Images/logo.png");
var image = new BitmapImage(logoUri);
```

## Formatos suportados:
- PNG (recomendado para logos com transparência)
- JPG/JPEG
- ICO (para ícones)
- SVG (pode precisar de biblioteca adicional)

