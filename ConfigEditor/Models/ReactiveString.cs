namespace ConfigEditor.Models;

public class ReactiveString
{
  public string Value { get; set; }

  public ReactiveString(string initialValue)
  {
    Value = initialValue;
  }
}
