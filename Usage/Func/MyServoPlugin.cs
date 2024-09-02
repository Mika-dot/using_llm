using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple.Usage.Func
{
    /// <summary>
    /// Управление сервоприводом
    /// </summary>
    /// <param name="turnedOn"></param>
    [Description("Represents a servo motor")]
    public class MyServoPlugin(int angle = 0)
    {
        private int _angle = angle;

        [KernelFunction, Description("Returns the current angle of the servo")]
        public int GetAngle() => _angle;

        [KernelFunction, Description("Sets the angle of the servo")]
        public void SetAngle(int angle) => _angle = angle;
    }
}
