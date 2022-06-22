using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Copier
    {
        // Deep copy array works very well. N-dimmension, [,,][][] etc. Any shape should be fine.
        // List<Array> or List<value> can be done deep.
        // Hashtag is fine.
        // Array of List does not work.
        // List of List works now.

        public static void DeepCopyClass(object class_src, object class_dest)
        {
            var members = class_src.GetType().GetFields();

            object ValB = null;

            foreach (var member in members)
            {
                var memberName = member.Name;

                var value = member.GetValue(class_src);
                var valType = member.FieldType;

                if (value != null)
                {
                    if (valType.IsArray)
                    {
                        ValB = DeepCopyArray((Array)value); //Handles jagged array forever!                      
                    }
                    else if (valType == typeof(List<>))
                    {
                        ValB = DeepCopyList(value);
                    }
                    else if (valType == typeof(HashSet<>))
                    {
                        ValB = DeepCopyHashSet(value);
                    }
                    else
                        ValB = (object)value;

                    class_dest.GetType().GetField(memberName).SetValue(class_dest, ValB);
                } //value != null
            } //member
        } //DeepCopy

        public static object DeepCopyHashSet(object hashTagObj)
        {
            var hashType = typeof(HashSet<>);
            var listObjType = hashTagObj.GetType();
            var itemType = hashTagObj.GetType().GetGenericArguments().Single();
            var constructedHashType = hashType.MakeGenericType(itemType);
            var hashCreated = Activator.CreateInstance(constructedHashType);
            hashCreated.GetType().GetMethod("UnionWith").Invoke(hashCreated, new object[] { hashTagObj });
            return hashCreated;
        }

        /// <summary>
        /// Works only for a list of value or Array for now.
        /// </summary>
        /// <param name="listObj"></param>
        /// <returns></returns>
        public static object DeepCopyList(object listObj)
        {
            var listType = typeof(List<>);
            var listObjType = listObj.GetType();
            var itemType = listObj.GetType().GetGenericArguments().Single();
            var constructedListType = listType.MakeGenericType(itemType);
            var listCreated = Activator.CreateInstance(constructedListType);

            var count = (int)(listObjType.GetProperty("Count").GetValue(listObj));
            Array arr = (Array)(listObjType.GetMethod("ToArray").Invoke(listObj, null));

            if (itemType.IsValueType)
            {
                listCreated.GetType().GetMethod("AddRange").Invoke(listCreated, new object[] { arr });
            }
            else if (itemType.IsArray)
            {
                for (int i = 0; i < count; i++)
                {
                    var val1 = arr.GetValue(i);
                    var arr1 = DeepCopyArray((Array)val1);
                    listCreated.GetType().GetMethod("Add").Invoke(listCreated, new object[] { arr1 });
                }
            }
            else if (itemType == typeof(List<>))
            {
                for (int i = 0; i < count; i++)
                {
                    var val1 = arr.GetValue(i); //Get reference.
                    var list1 = DeepCopyList(val1); //Deep copy the list.
                    listCreated.GetType().GetMethod("Add").Invoke(listCreated, new object[] { list1 });
                }
            }
            return listCreated;
        }

        /// <summary>
        /// Create array with same shape --- like [,,].
        /// Example is with shape of [3,2,4]
        /// </summary>
        /// <param name="sourceArray">Array input</param>
        /// <param name="arrayShape">Shape of array [3,2,4]</param>
        /// <param name="div">Number of elements after linearization in each element</param>
        /// <returns></returns>
        private static Array CreateArrayWithSameProperty(Array sourceArray, out int[] arrayShape, out int[] div)
        {
            var len0 = sourceArray.Length;
            var T = sourceArray.GetType();
            var T1 = T.GetElementType();
            Array arr = null;

            arrayShape = new int[sourceArray.Rank]; //[3,2,4]
            div = new int[sourceArray.Rank];

            for (int i = 0; i < sourceArray.Rank; i++)
            {
                arrayShape[i] = sourceArray.GetLength(i);
            }

            for (int i = 0; i < sourceArray.Rank; i++)
            {
                arrayShape[i] = sourceArray.GetLength(i);
                div[i] = 1;
            }

            for (int i = 0; i < sourceArray.Rank - 1; i++)
            {
                for (int j = i + 1; j < sourceArray.Rank; j++)
                    div[i] *= arrayShape[j];
                //i = 0 -> 1 * 2 * 4 = 8.
                //i = 1 -> 1 * 4 = 4.
                //i = 2 -> 1
            }

            arr = Array.CreateInstance(T1, arrayShape);

            return arr;
        }

        /// <summary>
        /// Deep copy any array, including Jagged array and Multi-dimension array. 
        /// Example is [3,2,4][2]
        /// Second example is [3,2,4][2][2,3]
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <returns></returns>
        public static Array DeepCopyArray(Array sourceArray)
        {
            Array arr;
            var elementType = sourceArray.GetType().GetElementType();

            if (!sourceArray.GetType().GetElementType().IsArray) //Both example: Array[2]. Skip. 
            {
                arr = (Array)sourceArray.Clone(); //2nd example, 2nd cycle, Array[2,3] comes to here.
            }
            else
            {
                arr = CreateArrayWithSameProperty(sourceArray, out int[] arrayShape, out int[] div); //arr[,,], lengthArr = [3,2,4], div = [8,4,1]
                for (int i = 0; i < sourceArray.Length; i++) //sourceArray.Length = 24
                {
                    var indx = new int[sourceArray.Rank]; //sourceArray.Rank = 3.
                    var rest = i; //example, i = 5,6,7,15,16,17 --> indx = [0,1,0]; [0,1,1], [0,1,2], [1,3,3],[2,0,0],[2,0,1]...
                    for (int j = 0; j < sourceArray.Rank; j++)
                    {
                        indx[j] = rest / div[j];
                        rest = rest % div[j];
                    }

                    var elem = (Array)(sourceArray.GetValue(indx)); //Array[2] for 1st example. Array[2][2,3] for 2nd.
                    if (elem != null)
                    {
                        if (elem.GetType().GetElementType().IsArray) //2nd example = Array[2,3]. So we will repeat.
                            arr.SetValue(DeepCopyArray(elem), indx);
                        else
                            arr.SetValue(elem.Clone(), indx); //1st example is scolar.
                    }
                }
            }
            return arr;
        }
    }
}
