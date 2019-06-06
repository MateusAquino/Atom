# Criando Falas
Para criar falas no arquivo `C:\Atom\Falas\falas.json`, respeitando as normas do JSON (veja [exemplo](https://jsonformatter.org/json-editor/3fbe7c)), adicione falas com estes requisitos:

 - `"fala"`: A mensagem que o usuário dirá em voz alta. (Para adicionar mais de uma, separe por Pipelines: `|`)
 - `"resposta"`: A mensagem que será reproduzida para certa fala.
 - `"cmd"`: Comandos (específicos) a serem executados para certa fala.
 - `"libera"`: Lista com novas falas a serem liberadas para certa fala.
 
## Comandos
 
### Tocar um vídeo
Sintaxe:  

    play <arquivo mp4>
     
Exemplo: `play C:/Atom/Tocador/am.mp4`  
Toca o vídeo e o mesmo é colocado no Plano de Fundo (Wallpaper).

### Parar vídeo
Sintaxe:

    unplay
    
Para o vídeo que estava sendo reproduzido.

### Atualizar plano de fundo
Sintaxe:

    atlz
    
Caso tenha alterado o `Wallpaper.html`, o comando `atlz` recarrega o plano de fundo.

## Libera (sistema de árvore/cadeia)

Exemplo do funcionamento do campo `"libera"`, para o JSON:

    [
      {
        "fala": "computador",
        "resposta": "sim mestre",
        "libera": [
          {
            "fala": "você namora?",
            "resposta": "quer mesmo saber mestre?",
            "libera": [
              {
                "fala": "sim|quero",
                "resposta": "até aqueles robôs da Disney namoram, e eu não..."
              }, {
                "fala": "não",
                "resposta": "ok, melhor assim!"
              }
            ]
          }
        ]
      },
      {
        "fala": "atom",
        "resposta": "me chamou pelo nome! aaaa que orgulho"
      }
    ]
    
### Versão imagem
![Exemplo de cadeia](https://i.ibb.co/mStyw9X/image.png)


A fala `"Sim"` e `"Não"` só podem ser executadas com precedencia das duas outras falas.  
Já `"Computador"` e `"Atom"` podem ser executadas a qualquer momento (Falas principais).
