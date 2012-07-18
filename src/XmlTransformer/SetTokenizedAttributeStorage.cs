using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    public class SetTokenizedAttributeStorage
    {
        public List<Dictionary<string, string>> DictionaryList { get; set; }

        public string TokenFormat { get; set; }

        public bool EnableTokenizeParameters { get; set; }

        public bool UseXpathToFormParameter { get; set; }

        public SetTokenizedAttributeStorage()
            : this(4)
        {
        }

        public SetTokenizedAttributeStorage(int capacity)
        {
            this.DictionaryList = new List<Dictionary<string, string>>(capacity);
            this.TokenFormat = "$(ReplacableToken_#(" + SetTokenizedAttributes.ParameterAttribute + ")_#(" + SetTokenizedAttributes.TokenNumber + "))";
            this.EnableTokenizeParameters = false;
            this.UseXpathToFormParameter = true;
        }
    }
}
