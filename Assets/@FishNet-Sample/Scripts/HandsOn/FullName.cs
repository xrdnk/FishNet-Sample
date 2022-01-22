namespace xrdnk.FishNet_Sample.Scripts.HandsOn
{
    /// <summary>
    /// フルネーム用のドメインオブジェクト
    /// </summary>
    public struct FullName
    {
        /// <summary>
        /// ファーストネーム（名）
        /// </summary>
        public string FirstName;

        /// <summary>
        /// ラストネーム（姓）
        /// </summary>
        public string LastName;

        /// <summary>
        /// ToString() オーバーライド処理
        /// </summary>
        /// <returns>フルネームで表示</returns>
        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}