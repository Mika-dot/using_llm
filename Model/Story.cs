using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple.Model
{
    /// <summary>
    /// Модель для хранения истории, роль и текст.
    /// </summary>
    public class Story
    {
        public Role Role { get; set; }
        public string Text { get; set; } = "Hmm, go on.";
        public Story(Role @role, string @text)
        {
            Role = @role;
            Text = @text;
        }

        /// <summary>
        /// Созданиея строчки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Role: {Role}, Text: {Text}";
        }
    }
}
