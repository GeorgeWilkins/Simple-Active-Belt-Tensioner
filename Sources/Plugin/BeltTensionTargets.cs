namespace User.ActiveBeltTensioner
{
    public struct BeltTensionTargets
    {
        public double LeftShoulder;
        public double RightShoulder;
        public double LeftWaist;
        public double RightWaist;

        public BeltTensionTargets(double leftShoulder, double rightShoulder, double leftWaist, double rightWaist)
        {
            LeftShoulder = leftShoulder;
            RightShoulder = rightShoulder;
            LeftWaist = leftWaist;
            RightWaist = rightWaist;
        }
    }
}
