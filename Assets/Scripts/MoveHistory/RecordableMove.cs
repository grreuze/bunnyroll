
public class RecordableMove {

    public RecordableObject reference;
    public enum eType {
        EntityState,
        EnterLevel,
        ExitLevel,
        StartEating,
        StopEating,
        ResetPoint
    }

    public eType type;

    public RecordableMove(RecordableObject reference, eType type) {
        this.reference = reference;
        this.type = type;
    }
}
