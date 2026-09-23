namespace Domain.Entities.Compartidos.Entity
{
  using System;

  /// <summary>
  /// Base class for entities.
  /// </summary>
  /// <typeparam name="T">
  /// Type of identity.
  /// </typeparam>
  public abstract class Entity<T> 
  {
    /// <summary>
    /// The requested hash code.
    /// </summary>
    private int? requestedHashCode;

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public virtual T Id
    {
      get;
      set;
    }

    /// <summary>
    /// The is valid identify.
    /// </summary>
    /// <param name="id">
    /// The id.
    /// </param>
    /// <returns>
    /// Is valid.
    /// </returns>
    public static bool IsValidId(object id)
    {
      if (id is int)
      {
        return (int)id > 0;
      }

      if (id is Guid)
      {
        return (Guid)id != Guid.Empty;
      }

      return false;
    }

    /// <summary>
    /// Check if this entity is without identity at this moment.
    /// </summary>
    /// <returns>True if entity is transient, else false.</returns>
    public bool IsIdentified()
    {
      return IsValidId(this.Id);
    }

    /// <summary>
    /// The get hash code.
    /// </summary>
    /// <returns>
    /// The System.Int32.
    /// </returns>
    public override int GetHashCode()
    {
      if (this.IsIdentified())
      {
        if (!this.requestedHashCode.HasValue)
        {
          this.requestedHashCode = this.Id.GetHashCode() ^ 31;
        }

        return this.requestedHashCode.Value;
      }

      return base.GetHashCode();
    }

    /// <summary>
    /// The equals.
    /// </summary>
    /// <param name="other">
    /// The other.
    /// </param>
    /// <returns>
    /// The System.Boolean.
    /// </returns>
    public bool Equals(Entity<T> other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      if (!other.IsIdentified() || !this.IsIdentified())
      {
        return false;
      }

      return other.Id.Equals(this.Id);
    }

    /// <summary>
    /// The key.
    /// </summary>
    /// <returns>
    /// The <see cref="object"/>.
    /// </returns>
    protected object GetKey()
    {
      return this.Id;
    }
  }
}
