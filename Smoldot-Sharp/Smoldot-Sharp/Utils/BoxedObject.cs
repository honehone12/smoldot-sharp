namespace SmoldotSharp
{
    public class BoxedObject
    {
        readonly object boxed;

        public BoxedObject(object toBox)
        {
            boxed = toBox;
        }

        public (bool, TUnmanaged) UnboxAsUnmanaged<TUnmanaged>() where TUnmanaged : unmanaged
        {
            if (boxed is TUnmanaged u)
            {
                return (true, u);
            }
            else
            {
                return (false, default(TUnmanaged));
            }
        }

        public (bool, string) UnboxAsString()
        {
            if (boxed is string s)
            {
                return (true, s);
            }
            else
            {
                return (false, string.Empty);
            }
        }

        public (bool, TStruct) UnboxAsStruct<TStruct>() where TStruct : struct
        {
            if (boxed is TStruct f)
            {
                return (true, f);
            }
            else
            {
                return (false, default(TStruct));
            }
        }

        public (bool, TClass) UnboxAsClass<TClass>() where TClass : class
        {
            if (boxed is TClass c)
            {
                return (true, c);
            }
            else
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return (false, null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
        }
    }
}