
using System.Reflection;

public class Singleton<T>
	where T : class
{
	protected Singleton() { }

	public static T Instance
	{
		get { return Nested.Instance; }
	}

	private sealed class Nested
	{
		static Nested()
		{
			Instance = typeof(T).InvokeMember(
				typeof(T).Name,
				BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				null,
				null
			) as T;
		}

		internal static T Instance { get; private set; }
	}
}
