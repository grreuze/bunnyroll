
public class ResetPoint : RecordableMove {

    public int index;

    public ResetPoint(RecordableObject reference, int index) : base(reference, eType.ResetPoint) {
        this.index = index;
    }

}
