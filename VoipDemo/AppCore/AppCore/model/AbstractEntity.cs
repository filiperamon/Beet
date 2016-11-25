using System;

namespace AppCore
{
	public abstract class AbstractEntity
	{
		public abstract long Cod{ get; set;}

		public override bool Equals (object obj)
		{
			if(obj == typeof(AbstractEntity)) {
				AbstractEntity otherEntity = (AbstractEntity)obj;
				return new EqualsBuilder ().Append (this.Cod, otherEntity.Cod).IsEquals ();
			}
			return base.Equals (obj);
		}

		public override int GetHashCode() {
			return new HashCodeBuilder().Append(Cod).ToHashCode();
		}

		public override string ToString()
		{
			string state = "transient";
			if (Cod != null) {
				state = Cod.ToString();
			}
			return this.GetType().BaseType.Name + ":" + state;
		}
	}
}

