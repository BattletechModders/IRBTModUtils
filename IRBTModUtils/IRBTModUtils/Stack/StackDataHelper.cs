using BattleTech;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace IRBTModUtils {
  public static class StackDataHelper {
    public static readonly string ABSTRACT_ACTOR_NAME = "AbstractActor";
    public static readonly string WEAPON_NAME = "Weapon";
    public static readonly string ATTACK_SEQUENCE_NAME = "attackSequence_id";
    public static readonly string MECH_DEF_NAME = "MechDef";
    private static ConcurrentDictionary<int, Dictionary<string, Stack<object>>> stack = new ConcurrentDictionary<int, Dictionary<string, Stack<object>>>();
    private static ConcurrentDictionary<int, Dictionary<string, int>> stackFlags = new ConcurrentDictionary<int, Dictionary<string, int>>();
    public static bool isFlagSet(this Thread thread, string flagName) {
      if (stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> flags)) {
        if (flags.TryGetValue(flagName, out int count)) {
          return count > 0;
        }
      }
      return false;
    }
    public static Dictionary<string, object> GetCurrentStack(this Thread thread) {
      Dictionary<string, object> result = new Dictionary<string, object>();
      if(stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> curStack) == false) {
        return result;
      }
      foreach(var stackItem in curStack) {
        if (stackItem.Value.Count == 0) { continue; }
        result.Add(stackItem.Key, stackItem.Value.Peek());
      }
      return result;
    }
    public static HashSet<string> GetCurrentFlags(this Thread thread) {
      HashSet<string> result = new HashSet<string>();
      if (stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> curStack) == false) {
        return result;
      }
      foreach (var stackItem in curStack) {
        if (stackItem.Value <= 0) { continue; }
        result.Add(stackItem.Key);
      }
      return result;
    }
    public static void SetFlag(this Thread thread, string flagName) {
      if (stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> flags) == false) {
        flags = new Dictionary<string, int>();
        stackFlags.AddOrUpdate(thread.ManagedThreadId, flags, (k,v)=> { return flags; });
      }
      if (flags.TryGetValue(flagName, out int count)) {
        flags[flagName] = count + 1;
      } else {
        flags.Add(flagName, 1);
      }
    }
    public static void ClearFlag(this Thread thread, string flagName) {
      if (stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> flags) == false) {
        flags = new Dictionary<string, int>();
        stackFlags.AddOrUpdate(thread.ManagedThreadId, flags, (k, v) => { return flags; });
      }
      if (flags.TryGetValue(flagName, out int count)) {
        flags[flagName] = count <= 1 ? 0 : count - 1;
      } else {
        flags.Add(flagName, 0);
      }
    }
    //public static void Clear() { stack.Clear(); stackFlags.Clear(); }
    public static void pushToStack<T>(this Thread thread, string name, T payload) {
      if (stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> thread_stack) == false) {
        thread_stack = new Dictionary<string, Stack<object>>();
        stack.AddOrUpdate(thread.ManagedThreadId, thread_stack, (k, v) => { return thread_stack; });
      }
      if (thread_stack.TryGetValue(name, out Stack<object> data_stack) == false) {
        data_stack = new Stack<object>();
        thread_stack.Add(name, data_stack);
      }
      data_stack.Push(payload);
    }
    public static void popFromStack<T>(this Thread thread, string name) {
      if (stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> thread_stack) == false) {
        thread_stack = new Dictionary<string, Stack<object>>();
        stack.AddOrUpdate(thread.ManagedThreadId, thread_stack, (k, v) => { return thread_stack; });
      }
      if (thread_stack.TryGetValue(name, out Stack<object> data_stack) == false) {
        data_stack = new Stack<object>();
        thread_stack.Add(name, data_stack);
      }
      data_stack.Pop();
    }
    public static T peekFromStack<T>(this Thread thread, string name) {
      if (stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> thread_stack) == false) {
        thread_stack = new Dictionary<string, Stack<object>>();
        stack.AddOrUpdate(thread.ManagedThreadId, thread_stack, (k, v) => { return thread_stack; });
      }
      if (thread_stack.TryGetValue(name, out Stack<object> data_stack) == false) {
        data_stack = new Stack<object>();
        thread_stack.Add(name, data_stack);
      }
      if (data_stack.Count > 0) { return (T)data_stack.Peek(); }
      //if (data_stack.TryPeek(out object data)) {
      //  return (T)data;
      //}
      return default(T);
    }
    public static int currentAttackSequence(this Thread thread) {
      return peekFromStack<int>(thread, ATTACK_SEQUENCE_NAME);
    }
    public static Mech currentMech(this Thread thread) {
      return peekFromStack<Mech>(thread, ABSTRACT_ACTOR_NAME);
    }
    public static Weapon currentWeapon(this Thread thread) {
      return peekFromStack<Weapon>(thread, WEAPON_NAME);
    }
    public static MechDef currentMechDef(this Thread thread) {
      return peekFromStack<MechDef>(thread, MECH_DEF_NAME);
    }
    public static PilotableActorDef currentPilotableActorDef(this Thread thread) {
      return peekFromStack<PilotableActorDef>(thread, MECH_DEF_NAME);
    }
    public static AbstractActor currentActor(this Thread thread) {
      return peekFromStack<AbstractActor>(thread, ABSTRACT_ACTOR_NAME);
    }
    public static void pushAttackSequenceId(this Thread thread, int id) {
      pushToStack<int>(thread, ATTACK_SEQUENCE_NAME, id);
    }
    public static void popAttackSequenceId(this Thread thread) {
      popFromStack<int>(thread, ATTACK_SEQUENCE_NAME);
    }
    public static void pushActor(this Thread thread, AbstractActor actor) {
      pushToStack<AbstractActor>(thread, ABSTRACT_ACTOR_NAME, actor);
    }
    public static void clearActor(this Thread thread) {
      popFromStack<AbstractActor>(thread, ABSTRACT_ACTOR_NAME);
    }
    public static void pushWeapon(this Thread thread, Weapon weapon) {
      pushToStack(thread, WEAPON_NAME, weapon);
    }
    public static void clearWeapon(this Thread thread) {
      popFromStack<Weapon>(thread, WEAPON_NAME);
    }
    public static void pushActorDef(this Thread thread, PilotableActorDef def) {
      pushToStack(thread, MECH_DEF_NAME, def);
    }
    public static void clearActorDef(this Thread thread) {
      popFromStack<PilotableActorDef>(thread, MECH_DEF_NAME);
    }
  }
}