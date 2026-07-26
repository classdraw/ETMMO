namespace ET.Server
{
    [Event(SceneType.Map)]
    public class NoticeBuffsToViewer_Handler : AEvent<Scene, NoticeBuffsToViewer>
    {
        protected override async ETTask Run(Scene scene, NoticeBuffsToViewer args)
        {
            Unit viewer = args.Viewer;
            Unit owner = args.Owner;
            if (viewer == null || viewer.IsDisposed || owner == null || owner.IsDisposed)
            {
                return;
            }

            BuffComponent buffComponent = owner.GetComponent<BuffComponent>();
            if (buffComponent == null || buffComponent.IsDisposed)
            {
                return;
            }

            await buffComponent.NoticeBuffsToViewer(viewer);
        }
    }
}
