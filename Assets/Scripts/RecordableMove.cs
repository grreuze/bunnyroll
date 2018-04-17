using UnityEngine;

public class RecordableMove {

    public enum eType {
        EntityState,
        EnterLevel,
        ExitLevel,
        StartEating,
        StopEating
    }

    public eType type;
}
