using System.Collections.Generic;
using System.Linq;
using ADV.Commands.Base;
using ADV.Commands.Camera;
using ADV.Commands.CameraEffect;
using ADV.Commands.Chara;
using ADV.Commands.Effect;
using ADV.Commands.EventCG;
using ADV.Commands.Game;
using ADV.Commands.H;
using ADV.Commands.HS2;
using ADV.Commands.Movie;
using ADV.Commands.Object;
using ADV.Commands.Sound.BGM;
using ADV.Commands.Sound.ENV;
using ADV.Commands.Sound.SE2D;
using ADV.Commands.Sound.SE3D;

namespace ADV;

public class CommandList : List<CommandBase>
{
	private TextScenario scenario { get; }

	public CommandList(TextScenario scenario)
	{
		this.scenario = scenario;
	}

	public new void Add(CommandBase item)
	{
	}

	public bool Add(ScenarioData.Param item, int currentLine)
	{
		CommandBase commandBase = CommandGet(item.Command);
		if (commandBase == null)
		{
			return true;
		}
		commandBase.Initialize(scenario, item.Command, item.Args);
		commandBase.ConvertBeforeArgsProc();
		for (int i = 0; i < commandBase.args.Length; i++)
		{
			if (commandBase.args[i].IsNullOrEmpty())
			{
				commandBase.args[i] = string.Empty;
			}
			commandBase.args[i] = scenario.ReplaceVars(commandBase.args[i]);
		}
		commandBase.localLine = currentLine;
		commandBase.Do();
		base.Add(commandBase);
		return item.Multi;
	}

	public bool Process()
	{
		this.Where((CommandBase item) => item.Process()).ToList().ForEach(delegate(CommandBase item)
		{
			Remove(item);
			item.Result(processEnd: true);
		});
		return this.Any((CommandBase p) => IsWait(p.command));
	}

	public void ProcessEnd()
	{
		ForEach(delegate(CommandBase item)
		{
			item.Result(processEnd: false);
		});
		Clear();
	}

	private static bool IsWait(Command command)
	{
		switch (command)
		{
		case Command.Choice:
		case Command.SceneFade:
		case Command.CharaKaraokePlay:
		case Command.TaskWait:
		case Command.SceneFadeWait:
		case Command.WaitAbs:
			return true;
		default:
			return false;
		}
	}

	public static CommandBase CommandGet(Command command)
	{
		return command switch
		{
			Command.Tag => new Tag(), 
			Command.Text => new Text(), 
			Command.Voice => new Voice(), 
			Command.Motion => new ADV.Commands.Base.Motion(), 
			Command.Expression => new ADV.Commands.Base.Expression(), 
			Command.VAR => new VAR(), 
			Command.RandomVar => new RandomVar(), 
			Command.IF => new IF(), 
			Command.Switch => new Switch(), 
			Command.Calc => new Calc(), 
			Command.Clamp => new Clamp(), 
			Command.Min => new Min(), 
			Command.Max => new Max(), 
			Command.Lerp => new Lerp(), 
			Command.LerpAngle => new LerpAngle(), 
			Command.InverseLerp => new InverseLerp(), 
			Command.LerpV3 => new LerpV3(), 
			Command.LerpAngleV3 => new LerpAngleV3(), 
			Command.Open => new Open(), 
			Command.Close => new Close(), 
			Command.Jump => new Jump(), 
			Command.Choice => new Choice(), 
			Command.Wait => new Wait(), 
			Command.TextClear => new Clear(), 
			Command.FontColor => new FontColor(), 
			Command.Scene => new Scene(), 
			Command.WindowActive => new WindowActive(), 
			Command.WindowImage => new WindowImage(), 
			Command.Regulate => new ADV.Commands.Base.Regulate(), 
			Command.Replace => new Replace(), 
			Command.Reset => new Reset(), 
			Command.Vector => new Vector(), 
			Command.Format => new Format(), 
			Command.NullLoad => new NullLoad(), 
			Command.NullRelease => new NullRelease(), 
			Command.NullSet => new NullSet(), 
			Command.BGLoad => new BackGroundLoad(), 
			Command.BGRelease => new BackGroundRelease(), 
			Command.BGVisible => new BackGroundVisible(), 
			Command.InfoAudioEco => new InfoAudioEco(), 
			Command.InfoAnimePlay => new InfoAnimePlay(), 
			Command.Fade => new ADV.Commands.Effect.Fade(), 
			Command.FadeColor => new FadeColor(), 
			Command.SceneFade => new ADV.Commands.Effect.SceneFade(), 
			Command.SceneFadeRegulate => new SceneFadeRegulate(), 
			Command.FilterImageLoad => new FilterImageLoad(), 
			Command.FilterImageRelease => new FilterImageRelease(), 
			Command.EjaculationEffect => new EjaculationEffect(), 
			Command.EcstacyEffect => new EcstacyEffect(), 
			Command.EcstacySyncEffect => new EcstacySyncEffect(), 
			Command.CrossFade => new ADV.Commands.CameraEffect.CrossFade(), 
			Command.CameraActive => new Active(), 
			Command.CameraAspect => new Aspect(), 
			Command.CameraChange => new Change(), 
			Command.CameraLerpNullMove => new LerpNullMove(), 
			Command.CameraLerpNullSet => new LerpNullSet(), 
			Command.CameraLerpAdd => new LerpAdd(), 
			Command.CameraLerpSet => new LerpSet(), 
			Command.CameraLerpAnimationAdd => new AnimationLerpAdd(), 
			Command.CameraLerpAnimationSet => new AnimationLerpSet(), 
			Command.CameraLerpTargetAdd => new LerpAddTarget(), 
			Command.CameraLerpTargetSet => new LerpSetTarget(), 
			Command.CameraPositionAdd => new ADV.Commands.Camera.AddPosition(), 
			Command.CameraPositionSet => new ADV.Commands.Camera.SetPosition(), 
			Command.CameraRotationAdd => new AddRotation(), 
			Command.CameraRotationSet => new SetRotation(), 
			Command.CameraDefault => new SetDefault(), 
			Command.CameraParent => new SetParent(), 
			Command.CameraLock => new Lock(), 
			Command.CameraGetFov => new GetFov(), 
			Command.CameraSetFov => new SetFov(), 
			Command.BGMPlay => new ADV.Commands.Sound.BGM.Play(), 
			Command.BGMStop => new ADV.Commands.Sound.BGM.Stop(), 
			Command.EnvPlay => new ADV.Commands.Sound.ENV.Play(), 
			Command.EnvStop => new ADV.Commands.Sound.ENV.Stop(), 
			Command.SE2DPlay => new ADV.Commands.Sound.SE2D.Play(), 
			Command.SE2DStop => new ADV.Commands.Sound.SE2D.Stop(), 
			Command.SE3DPlay => new ADV.Commands.Sound.SE3D.Play(), 
			Command.SE3DStop => new ADV.Commands.Sound.SE3D.Stop(), 
			Command.CharaCreate => new ADV.Commands.Chara.Create(), 
			Command.CharaFixCreate => new FixCreate(), 
			Command.CharaMobCreate => new MobCreate(), 
			Command.CharaDelete => new ADV.Commands.Chara.Delete(), 
			Command.CharaStand => new StandPosition(), 
			Command.CharaStandFind => new StandFindPosition(), 
			Command.CharaPositionAdd => new ADV.Commands.Chara.AddPosition(), 
			Command.CharaPositionSet => new ADV.Commands.Chara.SetPosition(), 
			Command.CharaPositionLocalAdd => new AddPositionLocal(), 
			Command.CharaPositionLocalSet => new SetPositionLocal(), 
			Command.CharaMotion => new ADV.Commands.Chara.Motion(), 
			Command.CharaMotionDefault => new MotionDefault(), 
			Command.CharaMotionWait => new MotionWait(), 
			Command.CharaMotionLayerWeight => new MotionLayerWeight(), 
			Command.CharaMotionSetParam => new MotionSetParam(), 
			Command.CharaMotionIKSetPartner => new MotionIKSetPartner(), 
			Command.CharaExpression => new ADV.Commands.Chara.Expression(), 
			Command.CharaFixEyes => new FixEyes(), 
			Command.CharaFixMouth => new FixMouth(), 
			Command.CharaGetShape => new GetShape(), 
			Command.CharaCoordinate => new Coordinate(), 
			Command.CharaClothState => new ClothState(), 
			Command.CharaSiruState => new SiruState(), 
			Command.CharaVoicePlay => new VoicePlay(), 
			Command.CharaVoiceStop => new VoiceStop(), 
			Command.CharaVoiceStopAll => new VoiceStopAll(), 
			Command.CharaVoiceWait => new VoiceWait(), 
			Command.CharaVoiceWaitAll => new VoiceWaitAll(), 
			Command.CharaLookEyes => new LookEyes(), 
			Command.CharaLookEyesTarget => new LookEyesTarget(), 
			Command.CharaLookEyesTargetChara => new LookEyesTargetChara(), 
			Command.CharaLookNeck => new LookNeck(), 
			Command.CharaLookNeckTarget => new LookNeckTarget(), 
			Command.CharaLookNeckTargetChara => new LookNeckTargetChara(), 
			Command.CharaLookNeckSkip => new LookNeckSkip(), 
			Command.CharaItemCreate => new ItemCreate(), 
			Command.CharaItemDelete => new ItemDelete(), 
			Command.CharaItemAnime => new ItemAnime(), 
			Command.CharaItemFind => new ItemFind(), 
			Command.EventCGSetting => new Setting(), 
			Command.EventCGRelease => new Release(), 
			Command.EventCGNext => new Next(), 
			Command.ObjectCreate => new ADV.Commands.Object.Create(), 
			Command.ObjectLoad => new Load(), 
			Command.ObjectDelete => new ADV.Commands.Object.Delete(), 
			Command.ObjectPosition => new ADV.Commands.Object.Position(), 
			Command.ObjectRotation => new Rotation(), 
			Command.ObjectScale => new Scale(), 
			Command.ObjectParent => new Parent(), 
			Command.ObjectComponent => new Component(), 
			Command.ObjectAnimeParam => new AnimeParam(), 
			Command.MoviePlay => new ADV.Commands.Movie.Play(), 
			Command.CharaActive => new CharaActive(), 
			Command.CharaVisible => new CharaVisible(), 
			Command.CharaColor => new CharaColor(), 
			Command.CharaChange => new CharaChange(), 
			Command.MapChange => new MapChange(), 
			Command.MapUnload => new MapUnload(), 
			Command.MapVisible => new MapVisible(), 
			Command.DayTimeChange => new DayTimeChange(), 
			Command.AddCollider => new AddCollider(), 
			Command.ColliderSetActive => new ColliderSetActive(), 
			Command.AddNavMeshAgent => new AddNavMeshAgent(), 
			Command.NavMeshAgentSetActive => new NavMeshAgentSetActive(), 
			Command.CameraLookAt => new ADV.Commands.Game.CameraLookAt(), 
			Command.MozVisible => new MozVisible(), 
			Command.LookAtDankonAdd => new LookAtDankonAdd(), 
			Command.LookAtDankonRemove => new LookAtDankonRemove(), 
			Command.HMotionShakeAdd => new MotionShakeAdd(), 
			Command.HMotionShakeRemove => new MotionShakeRemove(), 
			Command.BundleCheck => new BundleCheck(), 
			Command.CameraShakePos => new ShakePos(), 
			Command.CameraShakeRot => new ShakeRot(), 
			Command.Prob => new Prob(), 
			Command.Probs => new Probs(), 
			Command.FormatVAR => new FormatVAR(), 
			Command.CharaKaraokePlay => new KaraokePlay(), 
			Command.CharaKaraokeStop => new KaraokeStop(), 
			Command.Task => new Task(), 
			Command.TaskWait => new TaskWait(), 
			Command.TaskEnd => new TaskEnd(), 
			Command.Log => new Log(), 
			Command.CameraLightOffset => new CameraCorrectLightOffset(), 
			Command.CameraLightActive => new CameraCorrectLightActive(), 
			Command.CameraLightAngle => new CameraCorrectLightAngle(), 
			Command.CharaSetShape => new SetShape(), 
			Command.CharaCoordinateChange => new CoordinateChange(), 
			Command.CameraAnimeLoad => new AnimeLoad(), 
			Command.CameraAnimePlay => new AnimePlay(), 
			Command.CameraAnimeWait => new AnimeWait(), 
			Command.CameraAnimeLayerWeight => new AnimeLayerWeight(), 
			Command.CameraAnimeSetParam => new AnimeSetParam(), 
			Command.CameraAnimeRelease => new AnimeRelease(), 
			Command.InfoAudio => new InfoAudio(), 
			Command.CharaCreateEmpty => new CreateEmpty(), 
			Command.CharaCreateDummy => new CreateDummy(), 
			Command.CharaFixCreateDummy => new FixCreateDummy(), 
			Command.CharaMobCreateDummy => new MobCreateDummy(), 
			Command.ReplaceLanguage => new ReplaceLanguage(), 
			Command.SendCommandData => new SendCommandData(), 
			Command.SendCommandDataList => new SendCommandDataList(), 
			Command.IFVAR => new IFVAR(), 
			Command.CreateConcierge => new CreateConcierge(), 
			Command.SceneFadeWait => new SceneFadeWait(), 
			Command.DOFTargetMove => new DOFTargetMove(), 
			Command.DOFTargetSet => new DOFTargetSet(), 
			Command.BlurEffect => new BlurEffect(), 
			Command.DOFTarget => new DOFTarget(), 
			Command.SepiaEffect => new SepiaEffect(), 
			Command.VarLanguage => new VarLanguage(), 
			Command.TransitionFade => new TransitionFade(), 
			Command.TransitionFadeTexture => new TransitionFadeTexture(), 
			Command.SceneFadeColor => new SceneFadeColor(), 
			Command.SceneFadeTime => new SceneFadeTime(), 
			Command.MapObjectAnimation => new MapObjectAnimation(), 
			Command.NowLoadingDraw => new NowLoadingDraw(), 
			Command.DOFDefault => new DOFDefault(), 
			Command.WaitAbs => new Wait(), 
			Command.DOFAperture => new DOFAperture(), 
			Command.CharaWet => new CharaWet(), 
			Command.CreateSitri => new CreateSitri(), 
			Command.UseCorrectCamera => new UseCorrectCamera(), 
			_ => null, 
		};
	}
}
