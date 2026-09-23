using Domain.Context;

namespace Shared.AccessShared
{
  public abstract class DomainStore
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainService"/> class.
    /// </summary>
    /// <param name="context">
    /// The context.
    /// </param>
    protected DomainStore(SQLDBContext context)
    {
      this.Context = context;
    }

    /// <summary>
    /// Obtiene the context.
    /// </summary>
    public SQLDBContext Context { get; private set; }
  }
}
