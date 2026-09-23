namespace Shared.ClassShared.BusinessExceptions
{
  [Serializable]
  public class BusinessException: Exception
  {
    public BusinessRule ruleFail;
    public BusinessException(BusinessRule Rule) : base(Rule.Message) 
    {
      ruleFail = Rule;
    }
  }
}
