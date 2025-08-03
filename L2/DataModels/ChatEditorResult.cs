using System.Collections.Generic;

namespace ELOR.Laney.DataModels {
    public record class ChatEditorResult(int Id, string Name, string Description, Dictionary<string, string> Permissions);
}
