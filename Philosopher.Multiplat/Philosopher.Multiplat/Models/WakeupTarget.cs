namespace Philosopher.Multiplat.Models
{
    public class WakeupTarget
    {
        public string Hostname { get; set; }
        public int PortNumber { get; set; }
        public string MacAddress { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            WakeupTarget other = obj as WakeupTarget;
            if (other == null)
            {
                return false;
            }

            return (this.Hostname == other.Hostname
                && this.PortNumber == other.PortNumber
                && this.MacAddress == other.MacAddress);
        }

        public bool Equals(WakeupTarget other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return (this.Hostname == other.Hostname
               && this.PortNumber == other.PortNumber
               && this.MacAddress == other.MacAddress);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, Hostname) ? Hostname.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, PortNumber) ? PortNumber.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!object.ReferenceEquals(null, MacAddress) ? MacAddress.GetHashCode() : 0);
                return hash;
            }
        }

    }
}