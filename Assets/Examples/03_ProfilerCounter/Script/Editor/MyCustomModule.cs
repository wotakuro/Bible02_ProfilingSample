using System;
using Unity.Profiling;
using Unity.Profiling.Editor;

// ProfilerModule���p�������N���X���쐬���邱�ƂŁA�Ǝ���Module���쐬�ł��܂�
[Serializable]
[ProfilerModuleMetadata("MyCustomModule")]
public class MyCustomModule : ProfilerModule
{
    // �`���[�g�ɕ\������J�E���^�[��z��Ő錾���܂�
    static readonly ProfilerCounterDescriptor[] k_ChartCounters =
        new ProfilerCounterDescriptor[]{
        new ProfilerCounterDescriptor("�L�����̐�", ProfilerCategory.Scripts),
    };
    // �R���X�g���N�^�ŁAbase�N���X�ɃJ�E���^�[�ꗗ��n���ď��������܂�
    public MyCustomModule() : base(k_ChartCounters) { }
}

