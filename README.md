# using_llm

Файлы для чтения моделью
```sharp
public static List<string> FilePath = new List<string>() { 
    Path.GetFullPath(@"llm\help.pdf"),
    Path.GetFullPath((@"llm\instruction.pdf"))
        };
```
Использование
```sharp
UsageLLM lLM = new UsageLLM();

var text = await lLM.RunAsync("What does the red button mean?");
var text = lLM.Run("What does the red button mean?");
```
