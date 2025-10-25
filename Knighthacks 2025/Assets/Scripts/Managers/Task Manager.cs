using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TaskType { GrabTray, PickLeaves, PickGlass, BoilWater, PickSnack, DeliverFlower }
public enum TaskLocation { Front, Back }
public enum TaskState { Locked, Available, InProgress, Completed }

[Serializable]
public class OrderTask
{
    public TaskType type;
    public TaskLocation location;
    public TaskState state = TaskState.Locked;
    public string requiredItemId; // null/empty if no specific item required (e.g., GrabTray, BoilWater)

    public bool IsComplete => state == TaskState.Completed;
    public bool IsAvailable => state == TaskState.Available || state == TaskState.InProgress;
}

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    // Current sequence for active customer
    public List<OrderTask> currentTasks = new List<OrderTask>();

    // Events
    public event Action<OrderTask> TaskCompleted;
    public event Action AllTasksCompleted;
    public UnityEvent onOrderCompleted; // inspector hook

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Build the canonical task list for a customer order and unlock the first task.
    // customer.order format: [drink, snack, flower] (matches your project)
    public void StartTasksForCustomer(Customer customer)
    {
        if (customer == null) return;

        currentTasks.Clear();

        // 1. Grab tray (back)
        currentTasks.Add(new OrderTask { type = TaskType.GrabTray, location = TaskLocation.Back, requiredItemId = null });

        // 2. Pick tea leaves/flower from back (flower is index 2)
        string flowerId = (customer.order != null && customer.order.Length > 2) ? customer.order[2] : null;
        currentTasks.Add(new OrderTask { type = TaskType.PickLeaves, location = TaskLocation.Back, requiredItemId = flowerId });

        // 3. Pick glass size from front (use drink id as required item if you map it to a glass)
        string drinkId = (customer.order != null && customer.order.Length > 0) ? customer.order[0] : null;
        currentTasks.Add(new OrderTask { type = TaskType.PickGlass, location = TaskLocation.Front, requiredItemId = drinkId });

        // 4. Boil water from back
        currentTasks.Add(new OrderTask { type = TaskType.BoilWater, location = TaskLocation.Back, requiredItemId = null });

        // 5. Pick snack from front (snack is index 1)
        string snackId = (customer.order != null && customer.order.Length > 1) ? customer.order[1] : null;
        currentTasks.Add(new OrderTask { type = TaskType.PickSnack, location = TaskLocation.Front, requiredItemId = snackId });

        // 6. Pick flower and deliver from front (deliver the flower)
        currentTasks.Add(new OrderTask { type = TaskType.DeliverFlower, location = TaskLocation.Front, requiredItemId = flowerId });

        // Unlock first task
        if (currentTasks.Count > 0)
            currentTasks[0].state = TaskState.Available;
    }

    // Returns the first available (not completed) task, or null
    public OrderTask GetNextAvailableTask()
    {
        foreach (var t in currentTasks)
            if (t.IsAvailable) return t;
        return null;
    }

    // Attempt to complete a task. This is the API other systems should call when the player performs an action.
    // - `providedItemId` is the id/name of the item the player used (can be null for actions like BoilWater/GrabTray).
    // - `location` is where the player performed the action (front/back).
    // Returns true if a task was validated and marked completed.
    public bool TryCompleteTask(TaskType attemptedType, string providedItemId, TaskLocation location)
    {
        // find the earliest available task that matches type and location
        for (int i = 0; i < currentTasks.Count; i++)
        {
            var t = currentTasks[i];
            if (!t.IsAvailable) continue;
            if (t.type != attemptedType) continue;
            if (t.location != location) continue;

            // validate item if required
            if (!string.IsNullOrEmpty(t.requiredItemId))
            {
                if (string.IsNullOrEmpty(providedItemId)) return false; // item required but not provided
                if (!string.Equals(t.requiredItemId, providedItemId, StringComparison.OrdinalIgnoreCase)) return false; // wrong item
            }

            // passed checks -> mark complete
            t.state = TaskState.Completed;
            TaskCompleted?.Invoke(t);

            // Unlock next task in sequence (if any)
            UnlockNextTask(i + 1);

            // If all tasks completed, fire completed events
            if (AllTasksAreComplete())
            {
                AllTasksCompleted?.Invoke();
                onOrderCompleted?.Invoke();
            }
            return true;
        }

        return false;
    }

    private void UnlockNextTask(int startIndex)
    {
        for (int i = startIndex; i < currentTasks.Count; i++)
        {
            if (!currentTasks[i].IsComplete)
            {
                currentTasks[i].state = TaskState.Available;
                break;
            }
        }
    }

    private bool AllTasksAreComplete()
    {
        foreach (var t in currentTasks)
            if (!t.IsComplete) return false;
        return true;
    }

    // Utility to cancel/reset current task list
    public void CancelCurrentTasks()
    {
        currentTasks.Clear();
    }
}
