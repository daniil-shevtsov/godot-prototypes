namespace Prototypes.projection.asset.button;

public interface MyButton
{
    void Init(float resistance, float enabledThreshold);
    bool GetEnabled();
}