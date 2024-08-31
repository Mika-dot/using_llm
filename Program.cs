using LLama.Common;
using LLama;
using LLM__simple;
using LLM__simple.Model;
using LLM__simple.WorkingFiles;
using static System.Net.Mime.MediaTypeNames;
using LLM__simple.Word;
using LLM__simple.Usage;
using System.Text;


Singlet.Dialogue.Add(new Story(Role.System, "You are a translator. From English to Russian. Only translation into Russian without additions."));
Singlet.Dialogue.Add(new Story(Role.User, "Hi, my name is Bob."));
Singlet.Dialogue.Add(new Story(Role.Assistant, "Привет меня зовут Боб."));
Files.SaveToFile(Singlet.Dialogue);

List<Story> storList = Files.LoadFromFile();
Singlet.Dialogue = Conversion.GetLastNStoriesWithAssistantFirst(storList, 4);
Singlet.chatHistory = Conversion.StoryToHistory(Singlet.Dialogue);


var parameters = new ModelParams(Singlet.ModelPath)
{
    ContextSize = 1024,
    GpuLayerCount = 5
};

using var model = LLamaWeights.LoadFromFile(parameters);
using var context = model.CreateContext(parameters);
var executor = new InteractiveExecutor(context);
ChatSession session = new(executor, Singlet.chatHistory);

InferenceParams inferenceParams = new InferenceParams()
{
    MaxTokens = 256,
    AntiPrompts = Generate.ProcessStrings(Singlet.Anti)
};


while (true)
{
    string userInput = Console.ReadLine() ?? "";
    string textS = "";
    storList.Add(new Story(Role.User, userInput));

    await foreach (
        var text
        in session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userInput),
            inferenceParams))
    {
        Console.Write(text);
        textS += text;
    }
    storList.Add(new Story(Role.Assistant, StopWords.RemoveStopWords(textS)));

    userInput = Console.ReadLine();
}



UsageLLM lLM = new UsageLLM(null);

string meseg = lLM.Run("Hello World");