﻿namespace TehPers.PowerGrid.Units
{
    /// <summary>
    /// A measurement of power.
    /// </summary>
    public readonly struct Power
    {
        /// <summary>
        /// Zero power.
        /// </summary>
        public static Power Zero { get; } = default;

        /// <summary>
        /// One watt of power.
        /// </summary>
        public static Power OneWatt { get; } = new(81000);

        /// <summary>
        /// Quantity in units of 1/81000 of a watt.
        /// </summary>
        public long Units { get; }

        /// <summary>
        /// Quantity of power in watts.
        /// </summary>
        public long Watts => this / Power.OneWatt;

        private Power(long units)
        {
            this.Units = units;
        }

        public static Energy operator *(Power power, Duration time)
        {
            return new(checked(power.Units * time.Units));
        }

        public static long operator /(Power lhs, Power rhs)
        {
            return lhs.Units / rhs.Units;
        }

        public static Power operator -(Power power)
        {
            return new(-power.Units);
        }
    }
}