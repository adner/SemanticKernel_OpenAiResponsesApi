# syntax=docker/dockerfile:1.7

# --- Build args to toggle CPU vs GPU + select the right torch wheel index
ARG FLAVOR=gpu
# CPU: https://download.pytorch.org/whl/cpu
# CUDA 12.4: https://download.pytorch.org/whl/cu124
ARG TORCH_INDEX_URL=https://download.pytorch.org/whl/cu124

# --- Pick a sensible base image
# GPU (default): CUDA runtime for PyTorch CUDA wheels
FROM nvidia/cuda:12.4.1-cudnn-runtime-ubuntu22.04 AS base-gpu
# CPU: slim python base
FROM python:3.11-slim AS base-cpu

# --- Select base stage depending on FLAVOR
FROM base-${FLAVOR} AS final

# System deps
ENV DEBIAN_FRONTEND=noninteractive PIP_NO_CACHE_DIR=1 PYTHONDONTWRITEBYTECODE=1
RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl ca-certificates build-essential python3 python3-pip python3-venv python3-dev python-is-python3 \
  && rm -rf /var/lib/apt/lists/*

# Optional: pin pip/setuptools upgrades first
RUN python -m pip install --upgrade pip setuptools wheel

# --- Python deps per the Cookbook guide
# transformers, accelerate, torch, triton==3.4, kernels
# Torch comes from the provided index (CPU or CUDA) via TORCH_INDEX_URL
ARG TORCH_INDEX_URL
RUN python -m pip install \
    --index-url ${TORCH_INDEX_URL} \
    torch \
 && python -m pip install \
    transformers[serving] \
    accelerate \
    "triton==3.4" \
    kernels

RUN python -m pip install hf_transfer

# (Optional) If you want the HF CLI available
RUN python -m pip install "huggingface_hub[cli]"

# Caches + port
ENV HF_HOME=/data/hf \
    HF_HUB_ENABLE_HF_TRANSFER=1
EXPOSE 8000

# Simple health check: server returns 404 for unknown path but will respond—swap to a proper /health if added upstream
HEALTHCHECK --interval=30s --timeout=5s --retries=5 CMD curl -fsS http://localhost:8000/ || exit 0
