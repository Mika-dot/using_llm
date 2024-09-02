using LLamaSharp.KernelMemory;
using LLM__simple;
using LLM__simple.Usage;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using System.Diagnostics;



UsageLLM lLM = new UsageLLM();

var text = await lLM.RunAsync("What does the red button mean?");
var text = lLM.Run("What does the red button mean?");