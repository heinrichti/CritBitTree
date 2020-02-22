# Crit-bit Tree
Implements a binary crit-bit tree (PATRICIA tree). A crit-bit tree can be used as an alternative to a HashSet. 
In my tests this implementation similar to the .NET implementation of a HashSet for inserts and lookups. 
Faster in some situations, slower in others, so do your own testing and let me know if there is anything I can improve performance-wise.
Especially insert seems to be a little slow at the moment.

## Usage
```
using (var critBitTree = new CritBitTree())
{
    critBitTree.Add(test);
    if (critBitTree.Contains(test))
       ....
}
```
Currently only the methods `Add` and `Contains` are implemented, which take a Span<byte> each. 
This implementation makes heavy use of unmanaged memory, so the tree itself should be disposed after it is no longer needed.

Inspiration and details for this implementation can be found here: https://github.com/agl/critbit/blob/master/critbit.pdf