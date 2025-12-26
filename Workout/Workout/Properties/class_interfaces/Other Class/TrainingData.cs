namespace Workout.Properties.class_interfaces.Other
{
    public class TrainingData
    {
        private string animationUrl;
        private string name;
        private string shortDetail;
        private string longdetail;

        public TrainingData(string animationUrl, string name, string shortDetail, string longdetail)
        {
            this.animationUrl = animationUrl;
            this.name = name;
            this.shortDetail = shortDetail;
            this.longdetail = longdetail;
        }

        public string AnimationUrl => animationUrl;
        public string Name => name;
        public string ShortDetail => shortDetail;
        public string Longdetail => longdetail;
    }
}
