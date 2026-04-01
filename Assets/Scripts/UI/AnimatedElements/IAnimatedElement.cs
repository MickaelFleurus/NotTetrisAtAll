public interface IAnimatedElement
{
    AnimatedElementType GetAnimatedType();
    void Update(float deltaTime);
    public void Copy(UIAnimationData other);
}
