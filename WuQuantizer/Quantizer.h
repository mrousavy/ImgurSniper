#pragma once
Mark(struct box *cube, int label, unsigned char *tag);

int Cut(struct box *set1, struct box *set2);

float Maximize(cube, dir, first, last, cut,	whole_r, whole_g, whole_b, whole_w);

float Var(cube);

long int Top(cube, dir, pos, mmt);

long int Bottom(cube, dir, mmt);

long int Vol(cube, mmt);

void M3d(vwt, vmr, vmg, vmb, m2);

void Hist3d(vwt, vmr, vmg, vmb, m2);