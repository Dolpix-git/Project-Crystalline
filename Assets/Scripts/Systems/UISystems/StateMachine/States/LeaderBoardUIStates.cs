using FishNet;
using UnityEngine;

public class LeaderBoardUIStates : UIBaseState {
    public LeaderBoardUIStates(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) { }

    public override void OnEscape() {
        ctx.CurrentState = cashe.GetState(UIStates.Escape);
    }

    public override void OnDebug() {
        ctx.CurrentState = cashe.GetState(UIStates.Debug);
    }

    public override void OnLeaderBoardClose() {
        ctx.CurrentState = cashe.GetState(UIStates.Game);
    }

    public override void OnBuyMenu() {
        if (!CheckZone(PlayerManager.Instance.players[InstanceFinder.ClientManager.Connection].transform.position)) return;
        ctx.CurrentState = cashe.GetState(UIStates.BuyMenu);
    }

    public override UIStates GetState() {
        return UIStates.LeaderBoard;
    }
    private bool CheckZone(Vector3 pos) {
        Collider[] hits = Physics.OverlapSphere(pos, 0.1f);
        for (int i = 0; i < hits.Length; i++) {
            IZonable zone = hits[i].GetComponent<IZonable>();
            if (zone == null) continue;

            if (zone.GetZone() != Zones.Buy) continue;

            return true;
        }
        return false;
    }
}

